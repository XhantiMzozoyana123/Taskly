import { SearchDto } from '../dtos/SearchDto';
import { MessengerDto } from '../dtos/MessengerDto';
import { Lead } from '../domain/Leads'; // Assuming Lead will be the entity name

export interface IInstagramService {
  scrapePosts(searchDto: SearchDto): Promise<Lead[]>;
  checkIfContentIsRelevant(content: string, keywords: string[]): Promise<boolean>;
  sendDirectMessage(messengerDto: MessengerDto): Promise<boolean>;
}
