import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import {ActivatedRoute} from '@angular/router'
import { Router } from '@angular/router'; 
import { _throw } from 'rxjs/observable/throw'
import { SELECT_VALUE_ACCESSOR } from '../../../node_modules/@angular/forms/src/directives/select_control_value_accessor';

@Component({
  selector: 'app-event-details',
  templateUrl: './event-details.component.html',
  styleUrls: ['./event-details.component.css']
})
export class EventDetailsComponent implements OnInit {

  route_value:String
  recorddata = null

  constructor(private httpClient: HttpClient, private activeRoute: ActivatedRoute, private router: Router) { }

  ngOnInit() {
    this.activeRoute.params.subscribe(routeParams => {
      if (routeParams.id) {
        this.route_value = routeParams.id
        this.set_values()
      }
    });
  }
  set_values(){
    let endpoint = 'http://172.0.0.0:5000/Drillbit?filename='+this.route_value.split('.JPG')[0];
    this.httpClient
      .get(endpoint)
      .subscribe(
        data => { 
          this.recorddata = data['output']
        },
        error => this.handleError(error)
      );
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
