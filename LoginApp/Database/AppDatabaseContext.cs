using LoginApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace LoginApp.Data
{
    public class AppDatabaseContext : DbContext
    {
        public AppDatabaseContext(DbContextOptions<AppDatabaseContext> dbContext) : base(dbContext) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entityTypes = Assembly.GetExecutingAssembly().GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(ModelBase).IsAssignableFrom(t))
                    .ToList();

            foreach (var entityType in entityTypes)
            {
                var tableAttribute = entityType.GetCustomAttribute<TableAttribute>();
                if (tableAttribute is not null)
                {
                    modelBuilder.Entity(entityType).ToTable(tableAttribute.Name);

                    foreach (var property in entityType.GetProperties())
                    {
                        var isNotMapped = property.GetCustomAttribute<NotMappedAttribute>() != null;
                        if (isNotMapped)
                        {
                            modelBuilder.Entity(entityType).Ignore(property.Name);
                        } else
                        {
                            var columnName = ConvertToUpperSnakeCase(property.Name);
                            modelBuilder.Entity(entityType).Property(property.Name).HasColumnName(columnName);
                        }
                    }

                    var keyProperties = entityType.GetProperties().Where(x => x.GetCustomAttribute<KeyAttribute>() is not null).ToList();
                    if (keyProperties.Count == 1)
                    {
                        modelBuilder.Entity(entityType).HasKey(keyProperties.First().Name);
                    }
                    else if (keyProperties.Count > 1)
                    {
                        modelBuilder.Entity(entityType).HasKey(keyProperties.Select(x => x.Name).ToArray());
                    }
                }
            }

            base.OnModelCreating(modelBuilder);
        }

        private static string ConvertToUpperSnakeCase(string name)
        {
            var stringBuilder = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsUpper(c) && stringBuilder.Length > 0)
                {
                    stringBuilder.Append('_');
                }
                stringBuilder.Append(char.ToUpper(c));
            }
            return stringBuilder.ToString();
        }
    }
}
