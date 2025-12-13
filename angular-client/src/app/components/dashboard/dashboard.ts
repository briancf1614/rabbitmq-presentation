import { Component, signal } from '@angular/core';
import { CardData } from '../../models/image-request';
import { InfoCard } from "../info-card/info-card";

@Component({
  selector: 'app-dashboard',
  imports: [InfoCard],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
// Usa i Signal anche per i dati locali (Reactivity granulare)
  items = signal<CardData[]>([
    { id: 1, title: 'Server Alpha', description: 'Up and running', status: 'active' },
    { id: 2, title: 'Database', description: 'Backup in progress', status: 'pending' },
    { id: 3, title: 'Legacy System', description: 'Decommissioned', status: 'inactive' },
  ]);

  handleLog(id: number) {
    console.log('Click gestito dal padre per ID:', id);
  }
}
