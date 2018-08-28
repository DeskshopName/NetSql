# 1、说明

本项目是一个基于 Dapper 的轻量级 ORM 框架，目前只针对单表，不包含多表连接操作。

# 2、使用方法

## 2.2、安装

```csharp
Install-Package NetSql
```

## 2.2、创建实体

创建`Article`实体类，继承`EntityBase`

```csharp
public class Article : EntityBase
{
    [Column("Title")]
    public string Title1 { get; set; }

    public string Summary { get; set; }

    public string Body { get; set; }

    public Category Category { get; set; }

    public int ReadCount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedTime { get; set; }
}

public enum Category
{
    Blog,
    Movie
}
```

`EntityBase`是一个定义好的实体基类，包含一个泛型主键标识，默认是 Int 类型的，也可以指定 long 或者 string 类型

```csharp
 public class Article : EntityBase<string>
```

## 2.3、定义数据库上下文(DbContext)

数据库上下文我是模仿的 EF，`IDbContextOptions`是数据库上下文配置项接口，默认包含了 SqlServer 的实现`DbContextOptions`，如果使用的是 MySql 或者 SQLite，需要额外安装对应的扩展包

```csharp
Install-Package NetSql.MySql //MySql
```

```csharp
Install-Package NetSql.SQLite //SQLite
```

这里我定义了一个`BlogDbContext`上下文，其中包含一个`Articles`数据集

```csharp
public class BlogDbContext : DbContext
{
    public BlogDbContext(IDbContextOptions options) : base(options)
    {
    }

    public IDbSet<Article> Articles { get; set; }
}
```

## 2.4、数据集(DbSet)使用说明

### 2.4.1、创建数据库上下文实例

```csharp
private readonly BlogDbContext _dbContext;
private readonly IDbSet<Article> _dbSet;

public DbSetTests()
{
    _dbContext = new BlogDbContext(new SQLiteDbContextOptions("Filename=./Database/Test.db"));
    _dbSet = _dbContext.Set<Article>();

    //预热
    _dbSet.Find().First();
}
```

### 2.4.2、插入

```csharp
[Fact]
public async void InsertTest()
{
    var article = new Article
    {
        Title1 = "test",
        Category = Category.Blog,
        Summary = "这是一篇测试文章",
        Body = "这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章",
        ReadCount = 10,
        IsDeleted = true,
        CreatedTime = DateTime.Now
    };

    await _dbSet.InsertAsync(article);

    Assert.True(article.Id > 0);
}
```

### 2.4.3、批量插入

```csharp
[Fact]
public void BatchInsertTest()
{
    var sw = new Stopwatch();
    sw.Start();

    var tran = _dbContext.BeginTransaction();

    for (var i = 0; i < 10000; i++)
    {
        var article = new Article
        {
            Title1 = "test" + i,
            Category = i % 3 == 1 ? Category.Blog : Category.Movie,
            Summary = "这是一篇测试文章",
            Body = "这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章这是一篇测试文章",
            ReadCount = 10,
            IsDeleted = i % 2 == 0,
            CreatedTime = DateTime.Now
        };

        _dbSet.InsertAsync(article, tran);
    }

    tran.Commit();

    sw.Stop();

    var s = sw.ElapsedMilliseconds;

    Assert.True(s > 0);
}
```

### 2.4.4、根据主键删除

```csharp
[Fact]
public void DeleteTest()
{
    var b = _dbSet.DeleteAsync(3).Result;

    Assert.True(b);
}
```

### 2.4.5、根据表达式删除

```csharp
[Fact]
public async void DeleteWhereTest()
{
    var b = await _dbSet.Find(m => m.Id > 10).Delete();

    Assert.True(b);
}

[Fact]
public async void DeleteWhereTest()
{
    var b = await _dbSet.Find(m => m.Id > 10)
        .Where(m => m.CreatedTime > DateTime.Now).Delete();

    Assert.True(b);
}
```

### 2.4.6、修改

```csharp
[Fact]
public async void UpdateTest()
{
    var article = await _dbSet.Find().First();
    article.Title1 = "修改测试";

    var b = await _dbSet.UpdateAsync(article);

    Assert.True(b);
}
```

### 2.4.7、根据表达式修改实体部分属性

```csharp
[Fact]
public async void UpdateWhereTest()
{
    var b = await _dbSet.Find(m => m.Id == 1000).Update(m => new Article
    {
        Title1 = "hahahaah",
        ReadCount = 1000
    });

    Assert.True(b);
}
```

### 2.4.8、根据主键查询单个实体

```csharp
[Fact]
public void GetTest()
{
    var article = _dbSet.GetAsync(100).Result;

    Assert.NotNull(article);
}
```

### 2.4.9、根据表达式查询单条数据

该方法返回结果集中的第一条数据

```csharp
[Fact]
public async void GetWehreTest()
{
    var article = await _dbSet.Find(m => m.Id > 100).First();

    Assert.NotNull(article);
}
```

### 2.4.10、使用表达式

`IDbSet`的`Find`方法会返回一个`INetSqlQueryable`对象，这个对象是模仿的 EF 里面的`IQueryable`，虽然有些不伦不类，但是是按照适合自己的方式设计的。

**`INetSqlQueryable`目前包含以下方法：**

-   **`Where`**：用于添加过滤条件

```csharp
var query =  _dbSet.Find().Where(m => m.Id > 1);
```

-   **`WhereIf`**：根据指定条件来添加过滤条件

```csharp
var query = _dbSet.Find().WhereIf(id > 1, m => m.Id > 200);
```

-   **`OrderBy`**：用于添加排序规则

```csharp
var query = _dbSet.Find(m => m.Id > 200 && m.Id < 1000).OrderBy(m => m.Id, SortType.Desc);
```

-   **`Limit`**：该方法包含两个参数`skip`和`take`，标识跳过 skip 条数据，取 take 条数据

```csharp
var query = _dbSet.Find(m => m.Id > 100 && m.Id < 120).Limit(5, 10);
```

-   **`Select`**：选择要返回的列

```csharp
var query = _dbSet.Find().Select(m => new { m.Id, m.Title1 }).Limit(0, 10);
```

以上方法都是用于构造`INetSqlQueryable`的，下面的方法则是执行：

-   **`Max`**：查询最大值

```csharp
var maxReadCount = _dbSet.Find().Max(m => m.ReadCount).Result;
```

-   **`Min`**：查询最小值

```csharp
var maxReadCount = _dbSet.Find().Min(m => m.ReadCount).Result;
```

-   **`Count`**：查询数量

```csharp
var count = _dbSet.Find(m => m.Id > 1000).Count().Result;
```

-   **`Exists`**：判断是否存在

```csharp
var b = _dbSet.Find(m => m.Id > 1000).Exists().Result;
```

-   **`First`**：获取第一条数据

```csharp
var article = _dbSet.Find(m => m.Id > 100 && m.Id < 120).First().Result;
```

-   **`Delete`**：删除数据

```csharp
var b = _dbSet.Find(m => m.Id > 1000).Delete().Result;
```

-   **`Update`**：更新数据

```csharp
var b = await _dbSet.Find(m => m.Id == 1000).Update(m => new Article
{
    Title1 = "hahahaah",
    ReadCount = 1000
});
```

-   **`ToList`**：获取结果集

```csharp
var list = await _dbSet.Find(m => m.Id > 100 && m.Id < 120).ToList();
```

## 其它用法

### 表别名以及列名

```csharp
[Table("blog_article")]
public class Article : EntityBase
{
    [Column("Title")]
    public string Title1 { get; set; }

    public string Summary { get; set; }

    public string Body { get; set; }

    public Category Category { get; set; }

    public int ReadCount { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedTime { get; set; }
}
```

### 指定主键

可以通过`KeyAttribute`来指定某个字段为主键
