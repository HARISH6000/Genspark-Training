import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType, Chart, registerables } from 'chart.js';
import { User } from '../../services/user.service';

@Component({
  selector: 'app-role-bar-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './role-bar-chart.component.html',
  styleUrls: ['./role-bar-chart.component.css']
})
export class RoleBarChartComponent implements OnChanges {
  @Input() users: User[] = [];

  constructor() {
    Chart.register(...registerables);
  }

  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    scales: {
      x: {
        beginAtZero: true
      },
      y: {
        beginAtZero: true
      }
    },
    plugins: {
      legend: {
        display: false,
      }
    }
  };
  public barChartData: ChartData<'bar'> = {
    labels: [],
    datasets: [{
      data: [],
      label: 'Number of Users',
      backgroundColor: 'rgba(75, 192, 192, 0.6)',
      borderColor: 'rgba(75, 192, 192, 1)',
      borderWidth: 1
    }]
  };
  public barChartType: ChartType = 'bar';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['users'] && this.users.length > 0) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const roleCounts: { [key: string]: number } = {};
    this.users.forEach(user => {
      const role = user.role || 'Unspecified';
      roleCounts[role] = (roleCounts[role] || 0) + 1;
    });

    this.barChartData.labels = Object.keys(roleCounts);
    this.barChartData.datasets[0].data = Object.values(roleCounts);
    this.barChartData = { ...this.barChartData };
  }
}