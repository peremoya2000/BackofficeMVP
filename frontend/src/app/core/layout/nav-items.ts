export interface NavItem {
  path: string;
  label: string;
  icon: string;
}

export const NAV_ITEMS: readonly NavItem[] = [
  { path: '/dashboard', label: 'Panel', icon: 'dashboard' },
  { path: '/hotels', label: 'Hoteles', icon: 'hotel' },
];
