import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType, Chart, registerables } from 'chart.js';

@Component({
  selector: 'app-inventory-managers-bar-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './inventory-managers-bar-chart.html',
  styleUrls: ['./inventory-managers-bar-chart.css']
})
export class InventoryManagersBarChartComponent implements OnChanges {
  @Input() inventoryManagerCounts: { inventoryName: string; managerCount: number; location: string }[] = [];

  public barChartOptions: ChartConfiguration['options'];
  public barChartData: ChartData<'bar', number[], string>;
  public barChartType: ChartType = 'bar';

  constructor() {
    Chart.register(...registerables);

    this.barChartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      scales: {
        x: {
          title: {
            display: true,
            text: 'Inventory'
          }
        },
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: 'Number of Managers'
          },
          ticks: {
            stepSize: 1 // Managers are whole numbers
          }
        }
      },
      plugins: {
        legend: {
          display: false // No need for legend in a single-dataset bar chart
        },
        tooltip: {
          callbacks: {
            label: function(context) {
              const label = context.label || '';
              const value = (context.parsed.y as number) || 0;
              return `${label}: ${value} Managers`;
            }
          }
        }
      }
    };

    this.barChartData = {
      labels: [],
      datasets: [{
        data: [],
        label: 'Number of Managers',
        backgroundColor: 'rgba(0, 123, 255, 0.7)', // Using accent color
        borderColor: 'rgba(0, 123, 255, 1)',
        borderWidth: 1
      }]
    };
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['inventoryManagerCounts'] && this.inventoryManagerCounts) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    // Sort by managerCount in descending order and take top 10
    const sortedData = [...this.inventoryManagerCounts]
      .sort((a, b) => b.managerCount - a.managerCount)
      .slice(0, 10); // Take top 10

    const labels = sortedData.map(item => item.inventoryName);
    const data = sortedData.map(item => item.managerCount);

    this.barChartData = {
      labels: labels,
      datasets: [{
        data: data,
        label: 'Number of Managers',
        backgroundColor: 'rgba(0, 123, 255, 0.7)',
        borderColor: 'rgba(0, 123, 255, 1)',
        borderWidth: 1
      }]
    };

    // Trigger chart update
    this.barChartData = { ...this.barChartData };
  }
}
