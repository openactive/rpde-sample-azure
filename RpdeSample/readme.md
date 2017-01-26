# RpdeSample - C<span>#</span>

This is an example written to be run as an Azure Function. The `HttpTrigger` makes it incredibly easy to have your functions executed via an HTTP call to your function.

## How it works

This is a basic implementation of the [OpenActive RPDE standard](https://www.openactive.io/realtime-paged-data-exchange/).

When you call the function, be sure you checkout which security rules you apply. If you're using an apikey, you'll need to include that in your request.


## Todo

- Implement deleted flag