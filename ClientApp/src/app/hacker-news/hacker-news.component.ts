import { Component, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormControl } from '@angular/forms';

// dev commit change

//@Component({
//  selector: 'app-key-up3',
//  template: `
//    <input #box (keyup.enter)="onEnter(box.value)">
//    <p>{{value}}</p>
//  `
//})
//export class KeyUpComponent_v3 {
//  value = '';
//  onEnter(value: string) { this.value = value; }
//}

@Component({
  selector: 'app-hacker-news',
  templateUrl: './hacker-news.component.html'
})

export class HackerNewsComponent {
  public hackernews: HackerNews[] = [];
  public hackernewsdisplay: HackerNews[] = [];
  public hackernewssearch: HackerNews[] = [];
  public hackernewsitemcount: number = 101;
  public hackerarticleid: string = "25";
  public pagenum: number = 1;
  public dispcnt: number = 50;
  //public state: FormControl;

  constructor(http: HttpClient, @Inject('BASE_URL') baseUrl: string) {
    http.get<number>(baseUrl + 'hackernews/GetMaxStoryCount').subscribe(result => {
      this.hackernewsitemcount = result;
    }, error => console.error(error));
    console.log("Grab max count down");

    http.get<HackerNews[]>(baseUrl + 'hackernews/GetLatestPull').subscribe(
      result => {
        console.log("Grab Result");
        /*this.hackernews = result;*/
        for (var i = 0; i < this.hackernews.length; ++i) {
          this.hackernews.pop();
        }
        for (var i = 0; i < result.length; ++i) {
          this.hackernews.push(new HackerNewsObjectCreation(result[i].articleID, result[i].articleTitle, result[i].articleUrl));
        }
        console.log("copy results");

        this.hackernewsdisplay = [];

        for (var i = 0; i < 51; ++i) {
          this.hackernewsdisplay.push(this.hackernews[i]);
        }
        this.pagenum = 1;
        this.dispcnt = 50;

      }, error => console.error(error));

    //this.get50Article();
    //this.state.get['maxRows'].value
  }

  /*
  constructor(http: HttpClient) {
    console.log("Logging the HackerNewsComponent");
    http.get<number>('https://hacker-news.firebaseio.com/v0/maxitem.json').subscribe(data => {
      console.log(data);
      this.hackernewsitemcount = data;
      console.log(this.hackernewsitemcount);
    }, error => console.error(error + " !BIG ERROR!"));
    for (var i = 1; i < 200; ++i) {
      http.get<HackerNews>('https://hacker-news.firebaseio.com/v0/item/' + i + '.json').subscribe(data => {
        this.hackernews.push(data);
      }, error => console.error(error + " !BIG ERROR!"));
    }

  }*/

  public nextPage() {

    var startdex = 0 + this.dispcnt * this.pagenum + 1;
    if (startdex > this.hackernews.length) {
      return;
    }
    this.pagenum++;

    var enddex = this.dispcnt * this.pagenum + 1;
    if (enddex > this.hackernews.length)
      enddex = this.hackernews.length;

    this.hackernewsdisplay = [];

    for (var i = startdex; i < enddex; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }

  }

  public prevPage() {

    this.pagenum--;
    if (this.pagenum < 1)
      this.pagenum = 1;

    var startdex = (0 + this.dispcnt * this.pagenum + 1) - this.dispcnt;
    if (startdex < 0)
      startdex = 0;
    if (startdex > this.hackernews.length) {
      return;
    }

    var enddex = startdex + this.dispcnt;
    if (enddex > this.hackernews.length)
      enddex = this.hackernews.length;

    this.hackernewsdisplay = [];

    if (startdex == 1)
      startdex = 0;

    for (var i = startdex; i < enddex; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }

  }

  public get50Article() {
    this.hackernewsdisplay = [];

    for (var i = 0; i < 51; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }
    this.pagenum = 1;
    this.dispcnt = 50;
  }

  public get100Article() {
    this.hackernewsdisplay = [];

    for (var i = 0; i < 101; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }

    this.pagenum = 1;
    this.dispcnt = 100;
  }

  public get150Article() {
    this.hackernewsdisplay = [];

    for (var i = 0; i < 151; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }

    this.pagenum = 1;
    this.dispcnt = 150;
  }

  public get200Article() {
    //for (var i = 0; i < this.hackernewsdisplay.length; ++i) {
    //  this.hackernewsdisplay.pop();
    //}
    this.hackernewsdisplay = [];

    for (var i = 0; i < 201; ++i) {
      this.hackernewsdisplay.push(this.hackernews[i]);
    }

    this.pagenum = 1;
    this.dispcnt = 200;
  }

  public searchForArticles(s: string) {
    console.log("searching for " + s);
    this.hackernewsdisplay = [];

    //for (var i = 0; i < this.hackernewsdisplay.length; ++i) {
    //  this.hackernewsdisplay.pop();
    //}

    for (var i = 0; i < this.hackernews.length; ++i) {
      if (this.hackernews[i].articleTitle.includes(s) ||
        this.hackernews[i].articleUrl.includes(s)) {
        this.hackernewsdisplay.push(this.hackernews[i]);
      }
    }
  }
}

interface HackerNews {
  articleID: string;
  articleTitle: string;
  articleUrl: string;
  /*
  id: string;
  deleted: string;
  type: string;
  by: string;
  time: string;
  text: string;
  dead: string;
  parent: string;
  poll: string;
  kids: string;
  url: string;
  score: string;
  title: string;
  parts: string;
  descendants: string;
  */
}

class HackerNewsObjectCreation implements HackerNews {
  constructor(id: string, ti: string, ur: string) { this.articleID = id; this.articleTitle = ti; this.articleUrl = ur; }
    articleID: string;
    articleTitle: string;
    articleUrl: string;
}
