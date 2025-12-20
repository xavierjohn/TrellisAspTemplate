# Functional DDD Template

This is a clean architecture Domain-Driven Design (DDD) template that provides a structured foundation for building Web API applications using the functional programming paradigm.
The template follows the principles of DDD and includes the CQRS (Command Query Responsibility Segregation) pattern to separate write and read operations.

## Features

### Layered Architecture

The template promotes a layered architecture approach, consisting of the API layer, application layer, domain layer, and anti corruption layer. This separation of concerns allows for modular and testable code.

#### Domain-Driven Design

The template emphasizes the importance of capturing the business domain by implementing domain models and applying business rules within the domain layer. It promotes the use of a Ubiquitous Language and aggregates to model complex business concepts.

#### CQRS Pattern

The template leverages the CQRS pattern to separate write (command) operations from read (query) operations. This promotes scalability, performance optimization, and a clear separation of concerns between the different types of operations.

#### Anti-Corruption Layer (ACL)

The Anti-Corruption Layer acts as a bridge between the domain layer and external systems. The ACL helps to maintain the integrity and consistency of the domain model when interacting with external dependencies.

### Service Level Indicators

All the API methods will emit metrics with duration and status code to help monitor the service.

To view the metrics and traces locally run the docker container. 
Metrics can be viewed in [Prometheus](http://localhost:9090) and traces in [Zipkin](http://localhost:9411/zipkin/)

For details look at the package [ServiceLevelIndicators](https://github.com/xavierjohn/ServiceLevelIndicators)

### Railway Oriented Programming

Railway-oriented programming is an approach to error handling that is based on the idea of a railway track.
In this approach, the code is divided into a series of functions that represent different steps along the railway track.
Each function either succeeds and moves the code along the success track, or fails and sends the code down the failed track.
This approach can make error handling more explicit and easier to reason about.

For details look at the package [FunctionalDDD](https://github.com/xavierjohn/FunctionalDDD)

## Sample

Here is a more elaborate sample code [BuberDinner](https://github.com/xavierjohn/BuberDinner)

## Writing High Quality Services

[![Watch the video](https://img.youtube.com/vi/6_1CAgskxfM/hqdefault.jpg)](https://www.youtube.com/embed/6_1CAgskxfM)

## Getting Started

To get started with this template, All you need to do is download the repository to your local development environment, rename it to your project and use it. This template comes with all layers already implemented along with the Unit Tests in place.
This is not a Visual Studio project template.

### API Documentation

The template uses [Scalar](https://github.com/scalar/scalar) for interactive API documentation. When running in development mode, you can access the API documentation at `/scalar` which provides a modern, developer-friendly interface for exploring and testing your API endpoints.

The documentation is automatically generated from OpenAPI specifications and supports API versioning out of the box.

## Lets talk a little bit more about the layers implemented in the Template

### Domain Layer

The domain layer represents the core of the system. It consists of business models identified in the Ubiquitous Language of the domain. The domain layer uses aggregates, which are cohesive units of business logic and data. Aggregates enforce consistency boundaries and encapsulate the domain's behavior and rules. The domain layer focuses on capturing the essential concepts of the business and expressing them in a domain-specific language.

### Application Layer

The application layer, part of the domain, implements the business logic and uses abstractions to communicate with external systems. It follows the CQRS pattern, separating write operations (commands) from read operations (queries). The application layer processes commands, applying business rules and modifying the domain state. It also dispatches queries to retrieve data from read models. The application layer talks to external systems through abstractions, ensuring a decoupled and testable design.

### Anti-Corruption Layer

The Anti-Corruption Layer (ACL), which shields the domain layer from the complexities of external systems. The ACL translates and adapts external interfaces and data formats to match the domain's language and concepts. The infrastructure layer also encompasses components such as databases, external services, and other infrastructure-related code required for the system's operation. Infrastructure layer implements the abstractions defined in the application layer.

### API Layer

The API layer acts as the contract between the external world and the domain. It defines the communication endpoints and handles the serialization and deserialization of data. The API layer provides a standardized way for clients to send commands and queries to the system, adhering to the domain's language and concepts.

In summary, the API layer acts as the contract between the domain and the external world, the application layer implements the business logic using abstractions and follows the CQRS pattern, the domain layer represents the core business models and applies business rules, and the infrastructure layer is the ACL that translates the domain abstractions to external systems. This layered architecture with the CQRS pattern and the ACL helps separate concerns, maintain a clean design, and enable functional Domain-Driven Design practices.

## Convention over configuration for resource names

When deploying a service across multiple regions and environments, using convention over configuration for resource names can help prevent configuration errors.
By setting two configuration parameters, `RegionShortName` and `Environment`, resource names can be determined using `EnvironmentOptions` and its extensions.

The `Region` value can be used for the resource group name and in `LocationId` parameter of the Service Level Indicators.

Setting these values through configuration files or environment variables can simplify the process and reduce the need for numerous resource-specific settings in the configuration files.

```json
{
  "EnvironmentOptions": {
    "Environment": "test",
    "Region": "westus2",
    "RegionShortName": "usw2"
  },
}
```
