// ============================================================================
// EDU: ANDROID MVVM (Model-View-ViewModel)
// ============================================================================
// The ViewModel manages UI-related data in a lifecycle-conscious way.
// It survives configuration changes (like screen rotations).
// StateFlow provides a reactive stream of state that Jetpack Compose can observe,
// ensuring the UI always reflects the current data (Unidirectional Data Flow).
// ============================================================================

package com.example.androidapp.viewmodel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.URL
import java.io.OutputStreamWriter
import java.net.HttpURLConnection

sealed class OrderUiState {
    object Idle : OrderUiState()
    object Loading : OrderUiState()
    data class Success(val message: String) : OrderUiState()
    data class Error(val error: String) : OrderUiState()
}

class OrderViewModel : ViewModel() {
    private val _uiState = MutableStateFlow<OrderUiState>(OrderUiState.Idle)
    val uiState: StateFlow<OrderUiState> = _uiState.asStateFlow()

    fun submitOrder(customerName: String, amountString: String) {
        val amount = amountString.toDoubleOrNull() ?: 0.0

        viewModelScope.launch {
            _uiState.value = OrderUiState.Loading
            try {
                val result = withContext(Dispatchers.IO) {
                    sendOrderRequest(customerName, amount)
                }
                _uiState.value = OrderUiState.Success("Order Success: $result")
            } catch (e: Exception) {
                _uiState.value = OrderUiState.Error(e.message ?: "Unknown Error")
            }
        }
    }

    private fun sendOrderRequest(name: String, amount: Double): String {
        // 10.0.2.2 is standard for Android Emulator to host localhost
        val url = URL("http://10.0.2.2:5000/api/orders")
        val conn = url.openConnection() as HttpURLConnection
        conn.requestMethod = "POST"
        conn.setRequestProperty("Content-Type", "application/json; utf-8")
        conn.setRequestProperty("Accept", "application/json")
        conn.doOutput = true

        val jsonInputString = "{\"customerName\": \"$name\", \"totalAmount\": $amount}"

        OutputStreamWriter(conn.outputStream).use { os ->
            os.write(jsonInputString)
            os.flush()
        }

        if (conn.responseCode in 200..299) {
            return conn.inputStream.bufferedReader().use { it.readText() }
        } else {
            val errStr = conn.errorStream?.bufferedReader()?.use { it.readText() }
            throw Exception("Failed: ${conn.responseCode} - $errStr")
        }
    }
}
