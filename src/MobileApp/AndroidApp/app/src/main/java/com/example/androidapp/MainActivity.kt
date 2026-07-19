package com.example.androidapp

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.launch
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import java.net.URL
import java.io.OutputStreamWriter
import java.net.HttpURLConnection

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    OrderScreen()
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun OrderScreen() {
    var customerName by remember { mutableStateOf("") }
    var totalAmount by remember { mutableStateOf("") }
    var statusMessage by remember { mutableStateOf("") }
    val coroutineScope = rememberCoroutineScope()

    Column(modifier = Modifier.padding(16.dp)) {
        Text(text = "Enterprise Dashboard", style = MaterialTheme.typography.headlineMedium)
        Spacer(modifier = Modifier.height(16.dp))

        OutlinedTextField(
            value = customerName,
            onValueChange = { customerName = it },
            label = { Text("Customer Name") },
            modifier = Modifier.fillMaxWidth()
        )
        Spacer(modifier = Modifier.height(8.dp))

        OutlinedTextField(
            value = totalAmount,
            onValueChange = { totalAmount = it },
            label = { Text("Total Amount") },
            modifier = Modifier.fillMaxWidth()
        )
        Spacer(modifier = Modifier.height(16.dp))

        Button(
            onClick = {
                coroutineScope.launch {
                    statusMessage = "Sending..."
                    try {
                        val response = withContext(Dispatchers.IO) {
                            sendOrderRequest(customerName, totalAmount.toDoubleOrNull() ?: 0.0)
                        }
                        statusMessage = "Success: $response"
                    } catch (e: Exception) {
                        statusMessage = "Error: ${e.message}"
                    }
                }
            },
            modifier = Modifier.fillMaxWidth()
        ) {
            Text("Place Order")
        }

        Spacer(modifier = Modifier.height(16.dp))
        Text(text = statusMessage)
    }
}

fun sendOrderRequest(name: String, amount: Double): String {
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
        throw Exception("Failed with HTTP code: ${conn.responseCode}")
    }
}
