import { Component } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [MatCardModule, MatButtonModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  
  loginWithGoogle() {
    // Redireciona para o endpoint de autenticação Google do backend
    window.location.href = 'http://localhost:5000/api/auth/google-login';
  }
}

