import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<number>(baseUrl + 'hackernews/GetMaxStoryCount').subscribe(result => {
      console.log(result);
    }, error => console.error(error));
    console.log("Grab max count down");

    http.get<number>(baseUrl + 'hackernews/GrabRecentArticles/3000').subscribe(
      result => {
        console.log(result);
      }, error => console.error(error));
  }
}
