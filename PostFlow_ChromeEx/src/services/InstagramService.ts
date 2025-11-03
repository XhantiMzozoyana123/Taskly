import { IInstagramService } from '../interfaces/IInstagramService';
import { SearchDto } from '../dtos/SearchDto';
import { MessengerDto } from '../dtos/MessengerDto';
import { Lead } from '../domain/Leads';
import { API_ENDPOINTS } from '../constants/ApiConstants';

export class InstagramService implements IInstagramService {
  async scrapePosts(searchDto: SearchDto): Promise<Lead[]> {
    console.log(`Scraping Instagram for query: ${searchDto.query}`);
    // In a real scenario, this would involve content script communication
    // For now, we'll simulate some data and send it to the backend
    const scrapedLeads: Lead[] = [
      {
        name: 'Alice Wonderland',
        profileUrl: 'https://instagram.com/alicew',
        postDescription: 'Looking for creative content strategies.',
        postUrl: 'https://instagram.com/post3',
        platform: 'Instagram',
        keywords: searchDto.keywords.join(','),
        query: searchDto.query,
        status: 'New',
        postDate: new Date(),
      },
      {
        name: 'Bob The Builder',
        profileUrl: 'https://instagram.com/bobtheb',
        postDescription: 'Need help with social media advertising.',
        postUrl: 'https://instagram.com/post4',
        platform: 'Instagram',
        keywords: searchDto.keywords.join(','),
        query: searchDto.query,
        status: 'New',
        postDate: new Date(),
      },
    ];

    const relevantLeads: Lead[] = [];
    for (const lead of scrapedLeads) {
      const isRelevant = await this.checkIfContentIsRelevant(lead.postDescription, searchDto.keywords);
      if (isRelevant) {
        relevantLeads.push(lead);
        // Send lead to backend
        try {
          await fetch(API_ENDPOINTS.LEADS, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify(lead),
          });
          console.log('Lead saved to backend:', lead.name);
        } catch (error) {
          console.error('Error saving lead to backend:', error);
        }
      }
    }
    return relevantLeads;
  }

  async checkIfContentIsRelevant(content: string, keywords: string[]): Promise<boolean> {
    try {
      const response = await fetch(API_ENDPOINTS.AI_CHECK_RELEVANCE, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ content, topic: keywords.join(', ') }), // Assuming 'topic' for keywords
      });
      const result = await response.json();
      return result.isRelevant; // Assuming the backend returns { isRelevant: boolean }
    } catch (error) {
      console.error('Error checking content relevance with AI service:', error);
      return false; // Default to false on error
    }
  }

  async sendDirectMessage(messengerDto: MessengerDto): Promise<boolean> {
    try {
      const response = await fetch(API_ENDPOINTS.AI_GENERATE_DM, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(messengerDto),
      });
      const result = await response.json();
      // Assuming the backend returns { success: boolean, message: string } or similar
      console.log(`DM sent status for ${messengerDto.profileUrl}:`, result);
      return response.ok;
    } catch (error) {
      console.error('Error sending direct message via AI service:', error);
      return false;
    }
  }
}
