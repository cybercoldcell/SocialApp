import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { timeStamp } from 'console';

@Component({
  selector: 'app-server-error',
  templateUrl: './server-error.component.html',
  styleUrls: ['./server-error.component.css']
})
export class ServerErrorComponent implements OnInit {
  err: any;
  constructor(private router: Router) { 
    const navigation = this.router.getCurrentNavigation();
    this.err = navigation?.extras?.state?.error;
  }

  ngOnInit(): void {
  }

}
