export interface SearchDto {
  platform: 'Facebook' | 'Instagram';
  query: string;
  keywords: string[];
}
