using OrderProcessing.Shared.Storage;

var store = new JsonFileStore();
var seeder = new DataSeeder(store);
await seeder.ResetAsync();
