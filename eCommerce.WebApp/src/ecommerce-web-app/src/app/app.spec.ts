import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { App } from './app';

describe('App', () => {
  it('creates the application shell', async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([]), provideHttpClient()],
    }).compileComponents();
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
    expect(fixture.nativeElement.querySelector('app-shell')).toBeTruthy();
  });
});
