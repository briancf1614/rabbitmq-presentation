package com.example.androidapp

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.androidapp.viewmodel.OrderUiState
import com.example.androidapp.viewmodel.OrderViewModel

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
fun OrderScreen(viewModel: OrderViewModel = viewModel()) {
    var customerName by remember { mutableStateOf("") }
    var totalAmount by remember { mutableStateOf("") }

    val uiState by viewModel.uiState.collectAsState()

    Column(modifier = Modifier.padding(16.dp)) {
        Text(text = "Enterprise Dashboard (MVVM)", style = MaterialTheme.typography.headlineMedium)
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
            onClick = { viewModel.submitOrder(customerName, totalAmount) },
            enabled = uiState !is OrderUiState.Loading,
            modifier = Modifier.fillMaxWidth()
        ) {
            Text(if (uiState is OrderUiState.Loading) "Submitting..." else "Place Order")
        }

        Spacer(modifier = Modifier.height(16.dp))

        when (uiState) {
            is OrderUiState.Success -> Text(text = (uiState as OrderUiState.Success).message, color = MaterialTheme.colorScheme.primary)
            is OrderUiState.Error -> Text(text = (uiState as OrderUiState.Error).error, color = MaterialTheme.colorScheme.error)
            else -> {}
        }
    }
}
