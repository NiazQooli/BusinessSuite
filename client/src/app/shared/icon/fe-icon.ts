import { Component, Input } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import {
  lucideCheck,
  lucideCircleCheck,
  lucideCircleX,
  lucideEye,
  lucideFileDown,
  lucideFilePlus,
  lucidePencil,
  lucidePlus,
  lucideRotateCcw,
  lucideSave,
  lucideSearch,
  lucideSend,
  lucideTrash2,
  lucideUndo2,
  lucideX,
} from '@ng-icons/lucide';

export type FeIconName =
  | 'add'
  | 'edit'
  | 'delete'
  | 'save'
  | 'cancel'
  | 'post'
  | 'check'
  | 'find'
  | 'preview'
  | 'close'
  | 'pdf'
  | 'uncheck'
  | 'unpost'
  | 'row-add'
  | 'ok';

const ICON_MAP: Record<FeIconName, string> = {
  add: 'lucideFilePlus',
  edit: 'lucidePencil',
  delete: 'lucideTrash2',
  save: 'lucideSave',
  cancel: 'lucideCircleX',
  post: 'lucideSend',
  check: 'lucideCircleCheck',
  find: 'lucideSearch',
  preview: 'lucideEye',
  close: 'lucideX',
  pdf: 'lucideFileDown',
  uncheck: 'lucideRotateCcw',
  unpost: 'lucideUndo2',
  'row-add': 'lucidePlus',
  ok: 'lucideCheck',
};

@Component({
  selector: 'fe-icon',
  standalone: true,
  imports: [NgIcon],
  providers: [
    provideIcons({
      lucideFilePlus,
      lucidePencil,
      lucideTrash2,
      lucideSave,
      lucideCircleX,
      lucideSend,
      lucideCircleCheck,
      lucideSearch,
      lucideEye,
      lucideX,
      lucideFileDown,
      lucideRotateCcw,
      lucideUndo2,
      lucidePlus,
      lucideCheck,
    }),
  ],
  template: `<ng-icon class="fe-icon" [name]="iconName" [size]="size" [strokeWidth]="strokeWidth" />`,
  styles: [
    `
      :host {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
        line-height: 0;
        vertical-align: middle;
      }

      .fe-icon {
        display: block;
        color: currentColor;
      }
    `,
  ],
})
export class FeIconComponent {
  @Input({ required: true }) name!: FeIconName;
  @Input() size = '1.05rem';
  @Input() strokeWidth = '2.15';

  get iconName(): string {
    return ICON_MAP[this.name] ?? 'lucidePlus';
  }
}
