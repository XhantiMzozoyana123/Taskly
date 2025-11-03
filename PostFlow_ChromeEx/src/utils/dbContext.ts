// Basic IndexedDB helper for local storage
export class DbContext {
  private dbName: string = 'PostFlowDB';
  private dbVersion: number = 1;
  private db: IDBDatabase | null = null;

  constructor() {
    this.initDb();
  }

  private initDb(): Promise<void> {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(this.dbName, this.dbVersion);

      request.onupgradeneeded = (event) => {
        const db = (event.target as IDBOpenDBRequest).result;
        if (!db.objectStoreNames.contains('leads')) {
          db.createObjectStore('leads', { keyPath: 'id', autoIncrement: true });
        }
        // Add other object stores as needed
      };

      request.onsuccess = (event) => {
        this.db = (event.target as IDBOpenDBRequest).result;
        console.log('IndexedDB opened successfully');
        resolve();
      };

      request.onerror = (event) => {
        console.error('IndexedDB error:', (event.target as IDBOpenDBRequest).error);
        reject((event.target as IDBOpenDBRequest).error);
      };
    });
  }

  private getObjectStore(storeName: string, mode: IDBTransactionMode): IDBObjectStore {
    if (!this.db) {
      throw new Error('Database not initialized');
    }
    const transaction = this.db.transaction(storeName, mode);
    return transaction.objectStore(storeName);
  }

  async addLead(lead: any): Promise<number> {
    return new Promise((resolve, reject) => {
      const store = this.getObjectStore('leads', 'readwrite');
      const request = store.add(lead);

      request.onsuccess = () => resolve(request.result as number);
      request.onerror = () => reject(request.error);
    });
  }

  async getLeads(): Promise<any[]> {
    return new Promise((resolve, reject) => {
      const store = this.getObjectStore('leads', 'readonly');
      const request = store.getAll();

      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  // Add methods for querying/filtering by platform, query, keywords if needed for local storage
}
