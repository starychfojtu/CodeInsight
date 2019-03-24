# Pull Request index page

Whole page statistics can be configured by custom date range.
Every pull requests contributes to each day by its whole statistic.
As an example, a single pull request is represented by straight horizontal line starting at its creation date and ending at its merge/close date.

# Average pull request lifetime

This chart shows you the average total hours it gets to merge or close your pull requests.
This can help you predict how long it takes from opening a pull request to deploying it to production.
Longer lifetimes can indicate that you code review process is taking too long, and that maybe you don't discuss
design up front. It can also be, and usually is, that you pull requests are too big.

# Average efficiency

The average lifetime is handy, but it consideres easyfix of 1 line the same as a feature with 250 lines.
That is the cause of efficiency existence, which tries to show you your merged changes per hour ratio.
Although, the computation is not as simple as that.
Efficiency of pull request with C as total changes and H as life time in hours is computed as
100 * (Sigmoid(C / 1000)/ Max(1, Log10(H))), where Sigmoid(x) = 1/(1 + e^(-x)).
Sigmoid is great in terms of normalizing the size. Basically it says, that Pull requests with more than 1000 changes, have 
almost the same size. That prevents some generated code from drastically shifting your statistics.
The same is done with total hours, but in less drastic manner, for which logarithm is used.
This is of course not an objective ratio, but there is no such thing and as all other statistics, it should be viewed with grain of salt.
