import { Component } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { RatingsDataService } from '../ratings-data.service';

export interface TeamObject {
  name: string;
  rating: number;
  league: string;
  division: string;
  rank: number;
  imageURL: string;
}

function compareTeams(teamA: any, teamB: any) {
  if (parseInt(teamA.rating) >= parseInt(teamB.rating)) {
    return -1;
  } else {
    return 1;
  }
}

@Component({
  selector: 'app-table',
  standalone: true,
  imports: [MatTableModule],
  templateUrl: './table.component.html',
  styleUrl: './table.component.css',
})
export class TableComponent {
  displayedColumns: string[] = ['rank', 'imageUrl', 'name', 'rating'];
  dataSource: any[] = [];
  constructor(private ratingsDataService: RatingsDataService) {}
  fetchRatings(): void {
    this.ratingsDataService.getRatingsData().subscribe((data: any) => {
      data.sort(compareTeams);
      for (let i = 0; i < data.length; i++) {
        data[i].imageURL =
          data[i].cityName.toLowerCase().replace('.', '') +
          data[i].teamName.toLowerCase() +
          '.png';
        data[i].imageURL = data[i].imageURL.replace(/ /g, '');
        data[i].rank = i + 1;
        data[i].rating = Math.round(data[i].rating);
      }
      this.dataSource = data;
    });
  }

  ngOnInit(): void {
    this.fetchRatings();
  }
}
