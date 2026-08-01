import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AppShell } from './core/layout/app-shell/app-shell';

@Component({
  selector: 'app-root',
  imports: [AppShell],
  templateUrl: './app.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {}
