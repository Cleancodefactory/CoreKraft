<!-- header
{
    "title": "Profiling the server code",
    "description": "How to profile server execution and database access",
    "keywords": [ "Profiler", "Database", "profile", "time", "measure" ]
}
-->

## Built-in profiler for the server code
In CoreKraft we have wrapped the MiniProfiler. Thus, in case we want to replace it, the changes will affect only one library.
The CoreKraft profiler is defined in project: "Ccf.Ck.Utilities.Profiling".
By default CoreKraft initializes this profiler by calling the method: UseBindKraftProfiler(...).
CoreKraft has built-in profiling calls in a few places where all the requests go through:
```csharp
using (KraftProfiler.Current.Step("Execution time collecting parameters: "))
{
    ...
}
using (KraftProfiler.Current.Step("Execution time processing complete request: "))
{
    ...
}
using (KraftProfiler.Current.Step("Execution time loading data: "))
{
    ...
}
using (KraftProfiler.Current.Step("Execution time loading views: "))
{
    ...
}
```

## How to profile your own methods
If  you need more fine grained profiling info, you can add the above statements around the code you need to profile. Afterwards you will see the timing info together with your selected human readable description.


## How to visualize the profiler info
You can see the profiler both during debug, or in production.
The explanation will be for production, but you can use the profiler the same way, it's only a matter of Urls.
1. Launch the application
2. Launch a second browser window after you have been successfully authorized. Copy the Url from the first window into the second one
![Profile Production 1](../../../Images/ProfileProduction_1.png)
3. Replace (e.g. in our example https://server2.cleancode.factory/corekraftinternal/home/board home/board) with  

  a) "profiler/results-index"
    You will see the last 100 requests cached on the server. By clicking on the links you will drill down into more details.
    ![Profile Production Results Index](../../../Images/ProfileProduction_Results-Index.png)
    When you click on the link in the Name column you will drill down into the profiling details. The profiler collects additional info about the DBConnection object and stores it to the above mentioned C# objects. That means that your SQL queries are profiled and even duplicate execution is shown.
    By clicking back you will see the list again. The list is refreshed automatically with a minor delay, so no refresh needed.

  b) "profiler/results-list"
    This output can be used when you want to have the request timings in a concise list.
    ![Profile Production Results List](../../../Images/ProfileProduction_Results-List.png)

  c) "profiler/results"
    This Url will return only the latest request's details.
    ![Profile Production Results](../../../Images/ProfileProduction_Results.png)


