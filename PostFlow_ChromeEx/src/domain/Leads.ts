export interface Lead {
  id?: number; // Assuming an ID from BaseEntity
  campaignId?: number;
  name: string;
  profileUrl: string;
  postDescription: string;
  postUrl: string;
  platform: string;
  keywords: string;
  query: string;
  status: string;
  postDate: Date;
}
