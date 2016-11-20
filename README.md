# Interactive Pre-Generated Views for Entity Framework 6 - what is it?

[Entity Framework](https://github.com/aspnet/EntityFramework6) is an open source object-relational mapper from Microsoft which simplifies working with databases by eliminating the data access layer that would otherwise be needed to send queries/commands to the database and interpret the results. Entity Framework is not tied to any specific database but is using a provider model that allows plugging-in various databases by writing an EF provider for a given database. To be able to handle this, Entity Framework builds internally a set of views that describe the database in the database agnostic way. All further processing (queries, updates etc.) EF does is performed against these views. Generating views however can be costly and might impact start up time of the application. This can be worked around by generating views at design time e.g. by using [view generation templates](http://blog.3d-logic.com/2013/10/17/ef6-codefirst-view-generation-t4-template-for-c-updated-for-ef6-rtm/) or [EF Power Tools](http://blogs.msdn.com/b/adonet/archive/2013/10/12/ef-power-tools-beta-4-available.aspx). Views generated this way are typically static and may affect the build time of the project as well as the size of the application. An alternative is a solution where views are created dynamically only when needed (i.e. views don't exist or are out dated) and then persisted and made available for use by other instances of the same application (be it running in parallel or the same application after a restart). This is exactly the problem Interactive Pre Generated Views project is trying to solve. 

**This project was moved from https://efinteractiveviews.codeplex.com**

You may still find some useful information there:

- Old discussion board - https://efinteractiveviews.codeplex.com/discussions
- Issues - https://efinteractiveviews.codeplex.com/workitem/list/basic

# How to get it?

Interactive Pre Generated Views project ships on NuGet. You just need to add the [package](https://www.nuget.org/packages/EFInteractiveViews) to your project and set up a factory that generates views (see below). 

# How to use it?

Currently views can be stored either in a file on the disk or in the database. Regardless of the mechanism used to stored views the first step is to set up a factory that generates views. Note that this has to be done before sending the first query/command (including SaveChanges()) to the database since any of the operations will trigger view generation (for instance it can be done in the static constructor of the class that contains the Main method). 

The factory that generates views and persist them in a file can be registered as follows:

```C#
using (var ctx = new MyContext())
{
    InteractiveViews
        .SetViewCacheFactory(
            ctx, 
            new FileViewCacheFactory(@"C:\MyViews.xml"));
}
```

The factory that generates views and persist them in a database (note - currently only Sql Server is supported but other databases can be added very easily) can be registered as follows:

```C#
using (var ctx = new MyContext())
{
    InteractiveViews
        .SetViewCacheFactory(
            ctx, 
            new SqlServerViewCacheFactory(ctx.Database.Connection.ConnectionString));
}
```

*Note that the factory that generates and persists views in the database will create a table in the target database for storing views if the table does not exist. If the table cannot be created (e.g. the database does not exist or due to insufficient permissions) an exception will be thrown.*
