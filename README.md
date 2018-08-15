# Computation Expressions Workshop

The Computations Expressions Workshop collects and presents the content
of several papers and presents the material as a set of tutorials.
This content is intended to be used in a workshop setting.

## Setting up

The workshop uses the `dotnet` CLI as a base case. However, attendees may
elect to use an editor of their choice, though the steps to use those tools
will be omitted from the tutorials.

### Install `dotnet`

* Install the `dotnet` CLI through the [.NET Core SDK](https://www.microsoft.com/net/download)

NOTE: Some IDEs will also install the `dotnet` CLI.

### `git clone` Workshop Repository

Clone the workshop repository so you have the materials available locally.
Each tutorial is also tagged so that you can reference the finished result if you get stuck.
`git clone https://github.com/panesofglass/computation-expressions-workshop`

### Create a New Expecto Project

Now that you have the basics, you can get started by creating a new project with [Expecto](https://github.com/haf/expecto).

* Install the [Expecto](https://github.com/haf/expecto) template with `dotnet new -i Expecto.Template`
* Create a new project with `dotnet new expecto -lang fsharp`
