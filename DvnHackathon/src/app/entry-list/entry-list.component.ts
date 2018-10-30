import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {ActivatedRoute} from '@angular/router'
import {DatePipe} from '@angular/common'
import { Router } from '@angular/router';
import { _throw } from 'rxjs/observable/throw'
import 'rxjs/Rx';


@Component({
  selector: 'app-entry-list',
  templateUrl: './entry-list.component.html',
  styleUrls: ['./entry-list.component.css'],
  providers:[DatePipe]
})
export class EntryListComponent implements OnInit {

  recorddata  = null;

  constructor(private httpClient: HttpClient, private activeRoute: ActivatedRoute, public datepipe: DatePipe, private router: Router) {
  }

  getRecords(){
    let endpoint = 'http://127.0.0.0:5000/Images';
    this.httpClient
      .get(endpoint)
      .subscribe(
        data => { 
          this.recorddata = data['output']
        },
        error => this.handleError(error)
      );
  }

  ngOnInit() {
    this.getRecords()
  }

  private handleError(error: HttpErrorResponse) {
    alert("Error: Please try again later")
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong,
      console.error(
        `Backend returned code ${error.status}, ` +
        `body was: ${error.error}`);
    }
    // return an observable with a user-facing error message
    return _throw(
      'Something bad happened; please try again later.');
  };



}
