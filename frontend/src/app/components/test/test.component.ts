import { Component, OnInit } from '@angular/core';
import { ModalService } from '../../services/modal.service';

@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.sass']
})
export class TestComponent implements OnInit {

  constructor(private modalService: ModalService) { }

  ngOnInit(): void {
  }

  async callModal(){
    const res = await this.modalService.open();
    if (res){
      console.log('confirmed');
    }
    else{
      console.log('declined');
    }
  }
}
