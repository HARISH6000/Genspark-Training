import { Component, Input, OnChanges, SimpleChanges, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts'; // Ensure ng2-charts is installed
import { ChartConfiguration, ChartData, ChartType, Chart, registerables } from 'chart.js';

@Component({
  selector: 'app-inventory-product-pie-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './inventory-product-pie-chart.html', // Corrected template URL
  styleUrls: ['./inventory-product-pie-chart.css']
})
export class InventoryProductPieChartComponent implements OnChanges {
  @Input() inventoryProductCounts: { inventoryName: string; totalProducts: number; inventoryId: number }[] = [];

  public pieChartOptions: ChartConfiguration['options'];
  public pieChartData: ChartData<'pie', number[], string | string[]>;
  public pieChartType: ChartType = 'pie';

  constructor() {
    Chart.register(...registerables); // Register all Chart.js components

    this.pieChartOptions = {
      responsive: true,
      maintainAspectRatio: false, // Allows flexible sizing within container
      plugins: {
        legend: {
          display: true,
          position: 'right', // Display legend on the right
          labels: {
            usePointStyle: true, // Use a circle or square for the legend item
            font: {
              size: 12
            },
            // Custom label text to include inventory name and product count
            generateLabels: (chart) => {
              const data = chart.data;
              if (data.labels && data.datasets[0].data) {
                return data.labels.map((label, i) => {
                  const meta = chart.getDatasetMeta(0); // Access dataset meta
                  const element = meta.data[i]; // Access specific data element

                  const dataset = data.datasets[0];
                  const backgroundColor = Array.isArray(dataset.backgroundColor)
                    ? dataset.backgroundColor[i]
                    : dataset.backgroundColor;
                  const borderColor = Array.isArray(dataset.borderColor)
                    ? dataset.borderColor[i]
                    : dataset.borderColor;
                  const borderWidth = Array.isArray(dataset.borderWidth)
                    ? dataset.borderWidth[i]
                    : dataset.borderWidth;

                  const isHidden = !chart.isDatasetVisible(0) || (element && meta.hidden);

                  return {
                    text: `${label}: ${data.datasets[0].data[i] || 0} products`,
                    fillStyle: backgroundColor,
                    strokeStyle: borderColor,
                    lineWidth: borderWidth,
                    hidden: isHidden, // Check visibility safely
                    index: i,
                  };
                });
              }
              return [];
            },
          }
        },
        tooltip: {
          callbacks: {
            label: function (context) {
              const label = context.label || '';
              const value = (context.parsed as number) || 0; // Cast to number
              return `${label}: ${value.toFixed(0)} products`; // Show raw product count in tooltip
            }
          }
        }
      }
    };

    this.pieChartData = {
      labels: [],
      datasets: [{
        data: [],
        backgroundColor: [],
        hoverBackgroundColor: [],
        borderWidth: 1, // Add a slight border for better definition
        borderColor: '#ffffff'
      }]
    };
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['inventoryProductCounts'] && this.inventoryProductCounts) { // Ensure inventoryProductCounts is not null/undefined
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const labels: string[] = [];
    const data: number[] = [];
    const backgroundColors: string[] = [];
    const hoverBackgroundColors: string[] = [];

    // Use a color scale for more distinct colors, similar to Chart.js default colors
    const colorScale = [
      '#1f77b4', '#ff7f0e', '#2ca02c', '#d62728', '#9467bd',
      '#8c564b', '#e377c2', '#7f7f7f', '#bcbd22', '#17becf'
    ];

    this.inventoryProductCounts.forEach((item, index) => {
      // Only add to chart data if totalProducts is greater than 0
      if (item.totalProducts > 0) {
        labels.push(item.inventoryName);
        data.push(item.totalProducts);
        const colorIndex = index % colorScale.length;
        backgroundColors.push(colorScale[colorIndex]);
        hoverBackgroundColors.push(this.darkenColor(colorScale[colorIndex], 10)); // Darken by 20%
      }
    });

    this.pieChartData = {
      labels: labels,
      datasets: [{
        data: data,
        backgroundColor: backgroundColors,
        hoverBackgroundColor: hoverBackgroundColors,
        borderWidth: 1,
        borderColor: '#ffffff'
      }]
    };

    // Trigger chart update - by creating a new object reference
    this.pieChartData = { ...this.pieChartData };
  }

  // Helper function to darken a hex color (simple implementation)
  private darkenColor(hex: string, percent: number): string {
    // Validate and normalize the hex color
    let f = parseInt(hex.slice(1), 16); // Convert hex to integer
    if (isNaN(f) || hex.length !== 7 || hex[0] !== '#') {
      throw new Error('Invalid HEX color format. It should be in the form #RRGGBB.');
    }

    // Calculate darkened RGB components
    const t = percent < 0 ? 0 : 255;
    const p = Math.abs(percent) / 100;
    const R = (f >> 16) & 0xFF;
    const G = (f >> 8) & 0xFF;
    const B = f & 0xFF;

    // Apply the formula
    const newR = Math.round((t - R) * p + R);
    const newG = Math.round((t - G) * p + G);
    const newB = Math.round((t - B) * p + B);

    // Return as HEX
    return `#${(newR << 16 | newG << 8 | newB).toString(16).padStart(6, '0')}`;
  }

  get isNoData(): boolean {
    return (
      this.inventoryProductCounts.length === 0 ||
      this.inventoryProductCounts.every((d) => d.totalProducts === 0)
    );
  }
}
