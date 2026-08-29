import { Component, inject, OnInit, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { GraphItemComponent } from '../../components/graph-item/graph-item.component';
import { GraphItem } from '../../models/graph-item.model';
import { GraphItemService } from '../../services/graph-item.service';
import { GraphWizardDialogComponent } from '../../wizard/graph-wizard-dialog.component';

@Component({
  selector: 'app-dashboard',
  imports: [
    GraphItemComponent,
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly graphItemService = inject(GraphItemService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly graphItems = signal<GraphItem[]>([]);
  readonly isLoading = signal(true);
  readonly hasError = signal(false);

  ngOnInit(): void {
    this.loadGraphItems();
  }

  onCreateGraph(): void {
    this.dialog
      .open(GraphWizardDialogComponent, {
        width: 'min(52rem, calc(100vw - 2rem))',
        maxWidth: '52rem',
        autoFocus: 'first-tabbable',
        restoreFocus: true,
        panelClass: 'graph-wizard-dialog'
      })
      .afterClosed()
      .subscribe((created: GraphItem | undefined) => {
        if (!created) {
          return;
        }

        this.graphItems.update((items) => [...items, created]);
        this.snackBar.open('Graph created successfully.', 'Dismiss', { duration: 4000 });
      });
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
