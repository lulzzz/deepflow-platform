using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Deepflow.Common.Model
{
    public class ModelConfigurationLoader
    {
        public ModelConfiguration Load(string path)
        {
            var rows = File.ReadAllLines(path).Skip(1).Select(x => x.Split(",", StringSplitOptions.RemoveEmptyEntries)).Select(ToModelCsvRow);

            var entityTypes = new List<EntityTypeModel>
            {
                new EntityTypeModel { Guid = CreateGuidFromString("Field"), Name = "Field" },
                new EntityTypeModel { Guid = CreateGuidFromString("Manifold"), Name = "Manifold" },
                new EntityTypeModel { Guid = CreateGuidFromString("Well"), Name = "Well" }
            };

            var entityModels = rows.GroupBy(x => x.EntityName.ToLower()).Select(x => new EntityModel { Guid = CreateGuidFromString(x.Key), Name = x.First().EntityName, EntityTypeGuid = GetEntityTypeGuidFromName(x.First().EntityName) }).ToList();
            var attributeModels = rows.GroupBy(CreateEntityAttributeKey).Select(x => new AttributeModel { Guid = CreateGuidFromString(x.Key.ToLower()), Name = x.First().AttributeName, EntityTypeGuid = GetEntityTypeGuidFromName(x.First().EntityName), Unit = x.First().Unit }).ToList();
            var entityAttributeModels = rows.Select(row => new EntityAttributeModel { EntityGuid = CreateGuidFromString(row.EntityName), AttributeGuid = CreateGuidFromString(CreateEntityAttributeKey(row)), TagName = row.TagName }).ToList();

            return new ModelConfiguration
            {
                Entities = entityModels,
                Attributes = attributeModels,
                EntityAttributes = entityAttributeModels,
                Calculations = new List<CalculationModel>(),
                EntityTypes = entityTypes
            };
        }

        private Guid CreateGuidFromString(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input.ToLower()));
                return new Guid(hash);
            }
        }

        private EntityType GetEntityTypeFromName(string entityName)
        {
            if (entityName.ToLower().Contains("kel") || entityName.ToLower().Contains("yar"))
            {
                return EntityType.Well;
            }
            if (entityName.ToLower().Contains("kgf") || entityName.ToLower().Contains("ygf"))
            {
                return EntityType.Field;
            }
            if (entityName.ToLower().Contains("man"))
            {
                return EntityType.Manifold;
            }
            throw new Exception("Can't map entity name to type");
        }

        private Guid GetEntityTypeGuidFromName(string entityName)
        {
            if (entityName.ToLower().Contains("kel") || entityName.ToLower().Contains("yar"))
            {
                return CreateGuidFromString(EntityType.Well.ToString());
            }
            if (entityName.ToLower().Contains("kgf") || entityName.ToLower().Contains("ygf"))
            {
                return CreateGuidFromString(EntityType.Field.ToString());
            }
            if (entityName.ToLower().Contains("man"))
            {
                return CreateGuidFromString(EntityType.Manifold.ToString());
            }
            throw new Exception("Can't map entity name to type");
        }

        private string CreateEntityAttributeKey(ModelCsvRow row)
        {
            return GetEntityTypeFromName(row.EntityName) + ":" + row.AttributeName.ToLower();
        }

        private ModelCsvRow ToModelCsvRow(string[] line)
        {
            var entityNameIndex = 1;
            var attributeNameIndex = 2;
            var tagNameIndex = 5;
            var unitIndex = 3;

            var entityName = line[entityNameIndex];
            var attributeName = line[attributeNameIndex];
            var tagName = line[tagNameIndex];
            var unit = line[unitIndex];

            return new ModelCsvRow
            {
                EntityName = entityName,
                AttributeName = attributeName,
                TagName = tagName,
                Unit = unit
            };
        }

        public class ModelCsvRow
        {
            public string EntityName;
            public string AttributeName;
            public string TagName;
            public string Unit;
        }
    }
}
