using System.Collections.Generic;
using System.Linq;
using affolterNET.Data.DtoHelper.Database;
using affolterNET.Data.DtoHelper.Dialect;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace affolterNET.Data.DtoHelper.CodeGen
{
    public class ClassGenerator
    {
        private readonly List<MemberDeclarationSyntax> _list;
        private readonly Table _tbl;
        private readonly ISqlDialect _dialect;

        public ClassGenerator(Table tbl, ISqlDialect dialect)
        {
            _tbl = tbl;
            _dialect = dialect;
            _list = new List<MemberDeclarationSyntax>();
        }

        public ClassDeclarationSyntax Generate(ClassDeclarationSyntax classDeclaration)
        {
            // Table Name
            var tableNameConstant = _dialect.FormatTableNameConstant(_tbl.Schema, _tbl.Name);
            var sgTableName = new StringGenerator($"public const string TABLE_NAME = \"{tableNameConstant}\";");
            sgTableName.Generate(Add);

            // Properties für Attribute
            foreach (var col in _tbl.Columns)
            {
                var sgP = new StringGenerator(col.WriteProperty(0));
                sgP.Generate(Add);
            }

            var sgColumnNames = new ColumnNamesGenerator(_tbl, _dialect);
            sgColumnNames.Generate(Add);

            if (!_tbl.IsView)
            {
                // IsAutoincrement
                var sgAutoInc = new StringGenerator(_tbl.GetPrimaryKeyColumn().WriteIsAutoincrementId(0));
                sgAutoInc.Generate(Add);
            }

            // GetTableName
            var sgTn = new StringGenerator("public string GetTableName() { return TABLE_NAME; }");
            sgTn.Generate(Add);

            // SelectGenerator
            var sgSelect = new SelectGenerator(_tbl, _dialect);
            sgSelect.Generate(Add);

            if (!_tbl.IsView)
            {
                // InsertGenerator
                var sgInsert = new InsertGenerator(_tbl, _dialect);
                sgInsert.Generate(Add);

                // UpdateGenerator
                var sgUpdate = new UpdateGenerator(_tbl, _dialect);
                sgUpdate.Generate(Add);

                // DeleteGenerator
                var sgDelete = new DeleteGenerator(_tbl, _dialect);
                sgDelete.Generate(Add);

                // SaveByIdGenerator
                var sgSaveById = new SaveByIdGenerator(_tbl, _dialect);
                sgSaveById.Generate(Add);

                // Refresh()
                var sgRefresh = new RefreshGenerator(_tbl);
                sgRefresh.Generate(Add);

                // GetIdName
                var sgIdName = new StringGenerator(
                    $"public string GetIdName() {{ return \"{_tbl.GetPrimaryKeyColumn().PropertyName}\"; }}");
                sgIdName.Generate(Add);

                // SetId
                var sgSetId = new StringGenerator(_tbl.GetPrimaryKeyColumn().WriteSetId(0));
                sgSetId.Generate(Add);

                // SetInserted
                var dateTime = _tbl.InsertedUpdatedDateUtc ? "DateTime.UtcNow" : "DateTime.Now";
                var setInserted = new StringGenerator($@"public void SetInserted(string userName) {{
                    SetInsertedUser(userName);
                    SetInsertedDate({ dateTime });
                }}");
                setInserted.Generate(Add);

                // SetUpdated
                var setUpdated = new StringGenerator($@"public void SetUpdated(string userName) {{
                    SetUpdatedUser(userName);
                    SetUpdatedDate({ dateTime });
                }}");
                setUpdated.Generate(Add);

                // GetVersionName
                var sgVersionName =
                    new StringGenerator($"public string GetVersionName() {{ return \"{_tbl.GetVersionName()}\"; }}");
                sgVersionName.Generate(Add);

                // GetIsActiveName
                var sgGetIsActive = new StringGenerator(
                    $"public string GetIsActiveName() {{ return \"{_tbl.GetIsActiveName()}\"; }}");
                sgGetIsActive.Generate(Add);

                // SetIsActive
                var isActive = _tbl.GetIsActiveName();
                isActive = isActive == Constants.NotAvailable ? string.Empty : $"this.{ColPropertyName(isActive)} = isActive;";
                var sgSetIsActive = new StringGenerator($"public void SetIsActive(bool isActive) {{ {isActive} }}");
                sgSetIsActive.Generate(Add);

                // GetUpdatedUserName
                var sgUpdateUser = new StringGenerator(
                    $"public string GetUpdatedUserName() {{ return \"{_tbl.GetUpdatedUserName()}\"; }}");
                sgUpdateUser.Generate(Add);

                // SetUpdatedUser
                var updatedUser = _tbl.GetUpdatedUserName();
                updatedUser = updatedUser == Constants.NotAvailable ? string.Empty : $"this.{ColPropertyName(updatedUser)} = userName;";
                var sgSetUpdatedUser =
                    new StringGenerator($"public void SetUpdatedUser(string userName) {{ {updatedUser} }}");
                sgSetUpdatedUser.Generate(Add);

                // GetInsertedUserName
                var sgInsertUser = new StringGenerator(
                    $"public string GetInsertedUserName() {{ return \"{_tbl.GetInsertedUserName()}\"; }}");
                sgInsertUser.Generate(Add);

                // SetInsertedUser
                var insertedUser = _tbl.GetInsertedUserName();
                insertedUser = insertedUser == Constants.NotAvailable ? string.Empty : $"this.{ColPropertyName(insertedUser)} = userName;";
                var sgSetInsertedUser =
                    new StringGenerator($"public void SetInsertedUser(string userName) {{ {insertedUser} }}");
                sgSetInsertedUser.Generate(Add);

                // GetUpdatedDateName
                var sgUpdateDate = new StringGenerator(
                    $"public string GetUpdatedDateName() {{ return \"{_tbl.GetUpdatedDateName()}\"; }}");
                sgUpdateDate.Generate(Add);

                // SetUpdatedDate
                var updatedDate = _tbl.GetUpdatedDateName();
                updatedDate = updatedDate == Constants.NotAvailable ? string.Empty : $"this.{ColPropertyName(updatedDate)} = date;";
                var sgSetUpdatedDate =
                    new StringGenerator($"public void SetUpdatedDate(DateTime date) {{ {updatedDate} }}");
                sgSetUpdatedDate.Generate(Add);

                // GetInsertedDateName
                var sgInsertDate = new StringGenerator(
                    $"public string GetInsertedDateName() {{ return \"{_tbl.GetInsertedDateName()}\"; }}");
                sgInsertDate.Generate(Add);

                // SetInsertedDate
                var insertedDate = _tbl.GetInsertedDateName();
                insertedDate = insertedDate == Constants.NotAvailable ? string.Empty : $"this.{ColPropertyName(insertedDate)} = date;";
                var sgSetInsertedDate =
                    new StringGenerator($"public void SetInsertedDate(DateTime date) {{ {insertedDate} }}");
                sgSetInsertedDate.Generate(Add);
            }

            // ToString()
            var sgToString = new StringGenerator($"public override string ToString() {{ return $\"{_tbl}\";}}");
            sgToString.Generate(Add);

            classDeclaration = classDeclaration.AddMembers(_list.ToArray());

            return classDeclaration;
        }

        private string ColPropertyName(string columnName)
        {
            var col = _tbl.AllColumns.FirstOrDefault(c => c.Name == columnName);
            return col?.PropertyName ?? columnName;
        }

        private void Add(MemberDeclarationSyntax mds)
        {
            _list.Add(mds);
        }
    }
}
