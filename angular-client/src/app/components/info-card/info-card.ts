import { Component, input, Input, output } from '@angular/core';
import { CardData } from '../../models/image-request';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-info-card',
  imports: [FormsModule,CommonModule],
  templateUrl: './info-card.html',
  styleUrl: './info-card.scss',
})
export class InfoCard {
  data = input.required<CardData>(); 
  
  // Output moderno (opzionale, se ti serve emettere eventi)
  actionClicked = output<number>(); 

  onAction() {
    // Leggi il valore del signal con le parentesi ()
    this.actionClicked.emit(this.data().id);
  }
}
