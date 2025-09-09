import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface User {
  id: string;
  name: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private currentUserSubject: BehaviorSubject<User | null>;
  public currentUser: Observable<User | null>;

  constructor() {
    // Verifica se há um usuário logado no localStorage
    const storedUser = this.getStoredUser();
    this.currentUserSubject = new BehaviorSubject<User | null>(storedUser);
    this.currentUser = this.currentUserSubject.asObservable();
    
    // Para desenvolvimento: criar usuário mock se não existir
    if (!storedUser) {
      this.createMockUser();
    }
  }

  private createMockUser(): void {
    // Token JWT mock para desenvolvimento
    const mockToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwibmFtZSI6IlVzdWFyaW8gVGVzdGUiLCJlbWFpbCI6InRlc3RlQGV4YW1wbGUuY29tIiwiaWF0IjoxNTE2MjM5MDIyfQ.mock_signature';
    this.setUserData(mockToken, '1', 'Usuario Teste', 'teste@example.com');
  }

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get isAuthenticated(): boolean {
    return !!this.getToken() && !!this.currentUserValue;
  }

  public getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  public setUserData(token: string, userId: string, userName: string, userEmail: string): void {
    localStorage.setItem('jwt_token', token);
    localStorage.setItem('user_id', userId);
    localStorage.setItem('user_name', userName);
    localStorage.setItem('user_email', userEmail);

    const user: User = {
      id: userId,
      name: userName,
      email: userEmail
    };

    this.currentUserSubject.next(user);
  }

  public logout(): void {
    // Remove dados do localStorage
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('user_id');
    localStorage.removeItem('user_name');
    localStorage.removeItem('user_email');

    // Atualiza o subject
    this.currentUserSubject.next(null);
  }

  private getStoredUser(): User | null {
    const token = localStorage.getItem('jwt_token');
    const userId = localStorage.getItem('user_id');
    const userName = localStorage.getItem('user_name');
    const userEmail = localStorage.getItem('user_email');

    if (token && userId && userName && userEmail) {
      return {
        id: userId,
        name: userName,
        email: userEmail
      };
    }

    return null;
  }

  public getAuthHeaders(): { [key: string]: string } {
    const token = this.getToken();
    if (token) {
      return {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      };
    }
    return {
      'Content-Type': 'application/json'
    };
  }
}
