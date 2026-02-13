using System.Text.Json;
using DeepOverflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DeepOverflow.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<QuestionTag> QuestionTags => Set<QuestionTag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<EditHistory> EditHistories => Set<EditHistory>();
    public DbSet<Flag> Flags => Set<Flag>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ReputationHistory> ReputationHistory => Set<ReputationHistory>();
    public DbSet<BadgeDefinition> BadgeDefinitions => Set<BadgeDefinition>();
    public DbSet<UserBadge> UserBadges => Set<UserBadge>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ---- Core relational mappings (keep EF happy) ----
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // A Question may have an AcceptedAnswerId pointing to an Answer,
        // but that Answer is not "owned" by the relationship.
        modelBuilder.Entity<Question>()
            .HasOne(q => q.AcceptedAnswer)
            .WithMany()
            .HasForeignKey(q => q.AcceptedAnswerId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Question>()
            .HasOne(q => q.DuplicateOfQuestion)
            .WithMany()
            .HasForeignKey(q => q.DuplicateOfQuestionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Flag>()
            .HasOne(f => f.Flagger)
            .WithMany()
            .HasForeignKey(f => f.FlaggerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Flag>()
            .HasOne(f => f.Reviewer)
            .WithMany()
            .HasForeignKey(f => f.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        // Polymorphic relationships in Domain are not EF-mappable without extra tables.
        modelBuilder.Entity<Question>().Ignore(q => q.Comments);
        modelBuilder.Entity<Question>().Ignore(q => q.Votes);
        modelBuilder.Entity<Question>().Ignore(q => q.EditHistories);
        modelBuilder.Entity<Question>().Ignore(q => q.Flags);

        modelBuilder.Entity<Answer>().Ignore(a => a.Comments);
        modelBuilder.Entity<Answer>().Ignore(a => a.Votes);
        modelBuilder.Entity<Answer>().Ignore(a => a.EditHistories);
        modelBuilder.Entity<Answer>().Ignore(a => a.Flags);

        modelBuilder.Entity<Comment>().Ignore(c => c.Votes);

        modelBuilder.Entity<QuestionTag>()
            .HasKey(qt => new { qt.QuestionId, qt.TagId });

        modelBuilder.Entity<QuestionTag>()
            .HasOne(qt => qt.Question)
            .WithMany(q => q.QuestionTags)
            .HasForeignKey(qt => qt.QuestionId);

        modelBuilder.Entity<QuestionTag>()
            .HasOne(qt => qt.Tag)
            .WithMany(t => t.QuestionTags)
            .HasForeignKey(qt => qt.TagId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<Question>()
            .HasIndex(q => q.Slug)
            .IsUnique();

        modelBuilder.Entity<Vote>()
            .HasIndex(v => new { v.UserId, v.TargetType, v.TargetId })
            .IsUnique();

        // Store lists as jsonb (PostgreSQL)
        ConfigureJsonList(modelBuilder.Entity<User>().Property(u => u.Skills));
        ConfigureNullableJsonList(modelBuilder.Entity<EditHistory>().Property(e => e.BeforeTags));
        ConfigureNullableJsonList(modelBuilder.Entity<EditHistory>().Property(e => e.AfterTags));
    }

    private static void ConfigureJsonList<TElement>(
        PropertyBuilder<List<TElement>> propertyBuilder)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var converter = new ValueConverter<List<TElement>, string>(
            v => JsonSerializer.Serialize(v, jsonOptions),
            v => string.IsNullOrWhiteSpace(v)
                ? new List<TElement>()
                : (JsonSerializer.Deserialize<List<TElement>>(v, jsonOptions) ?? new List<TElement>()));

        var comparer = new ValueComparer<List<TElement>>(
            (a, b) => SequenceEqualList(a, b),
            v => GetListHash(v),
            v => v.ToList());

        propertyBuilder
            .HasConversion(converter)
            .Metadata.SetValueComparer(comparer);

        // Best-effort for PostgreSQL; harmless on other providers.
        propertyBuilder.HasColumnType("jsonb");
    }

    private static void ConfigureNullableJsonList<TElement>(
        PropertyBuilder<List<TElement>?> propertyBuilder)
    {
        var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        var converter = new ValueConverter<List<TElement>?, string?>(
            v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
            v => string.IsNullOrWhiteSpace(v)
                ? null
                : JsonSerializer.Deserialize<List<TElement>>(v, jsonOptions));

        var comparer = new ValueComparer<List<TElement>?>(
            (a, b) => NullableSequenceEqualList(a, b),
            v => GetNullableListHash(v),
            v => v == null ? null : v.ToList());

        propertyBuilder
            .HasConversion(converter)
            .Metadata.SetValueComparer(comparer);

        propertyBuilder.HasColumnType("jsonb");
    }

    private static bool SequenceEqualList<TElement>(List<TElement> a, List<TElement> b)
        => a.SequenceEqual(b);

    private static bool NullableSequenceEqualList<TElement>(List<TElement>? a, List<TElement>? b)
        => a == null ? b == null : (b != null && a.SequenceEqual(b));

    private static int GetListHash<TElement>(List<TElement> v)
    {
        var hash = 0;
        foreach (var x in v)
        {
            hash = HashCode.Combine(hash, x == null ? 0 : x.GetHashCode());
        }
        return hash;
    }

    private static int GetNullableListHash<TElement>(List<TElement>? v)
        => v == null ? 0 : GetListHash(v);
}

