import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType, Chart, registerables } from 'chart.js';
import { User } from '../../services/user.service';

@Component({
  selector: 'app-gender-pie-chart',
  standalone: true,
  imports: [CommonModule, BaseChartDirective],
  templateUrl: './gender-pie-chart.component.html',
  styleUrls: ['./gender-pie-chart.component.css']
})
export class GenderPieChartComponent implements OnChanges {
  @Input() users: User[] = [];

  constructor() {
    Chart.register(...registerables);
  }

  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'top',
      },
      tooltip: {
        callbacks: {
          label: function(context) {
            let label = context.label || '';
            if (label) {
              label += ': ';
            }
            if (context.parsed !== null) {
              label += context.parsed + '%';
            }
            return label;
          }
        }
      }
    }
  };
  public pieChartData: ChartData<'pie', number[], string | string[]> = {
    labels: ['Male', 'Female', 'Other'],
    datasets: [{
      data: [],
      backgroundColor: ['#42A5F5', '#FF4081', '#FFEB3B'],
      hoverBackgroundColor: ['#64B5F6', '#FF80AB', '#FFEE58'] 
    }]
  };
  public pieChartType: ChartType = 'pie';

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['users'] && this.users.length > 0) {
      this.updateChartData();
    }
  }

  private updateChartData(): void {
    const genderCounts = { male: 0, female: 0, other: 0 };
    this.users.forEach(user => {
      const gender = user.gender?.toLowerCase();
      if (gender === 'male') {
        genderCounts.male++;
      } else if (gender === 'female') {
        genderCounts.female++;
      } else {
        genderCounts.other++;
      }
    });

    const totalUsers = this.users.length;
    const malePercentage = totalUsers > 0 ? (genderCounts.male / totalUsers) * 100 : 0;
    const femalePercentage = totalUsers > 0 ? (genderCounts.female / totalUsers) * 100 : 0;
    const otherPercentage = totalUsers > 0 ? (genderCounts.other / totalUsers) * 100 : 0;

    this.pieChartData.datasets[0].data = [malePercentage, femalePercentage, otherPercentage];
    this.pieChartData.labels = [`Male (${genderCounts.male})`, `Female (${genderCounts.female})`, `Other (${genderCounts.other})`];
    this.pieChartData = { ...this.pieChartData };
  }
}