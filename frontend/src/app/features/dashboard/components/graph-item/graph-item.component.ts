import { Component, input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { GraphItem } from '../../models/graph-item.model';

@Component({
  selector: 'app-graph-item',
  imports: [MatCardModule],
  templateUrl: './graph-item.component.html',
  styleUrl: './graph-item.component.scss'
})
export class GraphItemComponent {
  readonly graphItem = input.required<GraphItem>();
}
