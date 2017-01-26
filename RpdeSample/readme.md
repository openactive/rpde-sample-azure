# RpdeSample - C<span>#</span>

This is an example written to be run as an Azure Function. The `HttpTrigger` makes it incredibly easy to have your functions executed via an HTTP call to your function.

## What is this?

This is a basic implementation of the [OpenActive RPDE standard](https://www.openactive.io/realtime-paged-data-exchange/).

When you call the function, be sure you checkout which security rules you apply. If you're using an apikey, you'll need to include that in your request.

## How do I get it working?

This function runs against Microsoft Azure's sample database "AdventureWorksLT", which can be created from inside Azure by adding a new database while selecting Source as "Sample (AdventureWorksLT)".

To configure this function to access the database, simply create a new Connection String named "sqldb_connection" under your cloud function's Application Settings. You can find the value from the "Database connection strings" Quick Start within the new sample SQL Database.


## Todo

- Implement deleted flag
