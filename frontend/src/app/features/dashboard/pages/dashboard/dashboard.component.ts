import { Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GraphItemComponent } from '../../components/graph-item/graph-item.component';
import { GraphItem } from '../../models/graph-item.model';
import { GraphItemService } from '../../services/graph-item.service';

@Component({
  selector: 'app-dashboard',
  imports: [GraphItemComponent, MatButtonModule, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly graphItemService = inject(GraphItemService);

  readonly graphItems = signal<GraphItem[]>([]);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);

  ngOnInit(): void {
    this.loadGraphItems();
  }

  onCreateGraph(): void {
    // Wizard will be added in a follow-up step.
  }

  retry(): void {
    this.loadGraphItems();
  }

  private loadGraphItems(): void {
    this.isLoading.set(true);
    this.hasError.set(false);

    this.graphItemService.getGraphItems().subscribe({
      next: (items) => {
        this.graphItems.set(items);
        this.isLoading.set(false);
      },
      error: () => {
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }
}
