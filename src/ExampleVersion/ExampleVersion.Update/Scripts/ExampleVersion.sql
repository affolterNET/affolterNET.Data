create schema ExampleVersion
    
go

create table ExampleVersion.T_DemoTableType(
    Id uniqueidentifier not null
        constraint PK_DemoTableType
            primary key clustered,
    Name nvarchar(1000) not null,
)

create table ExampleVersion.T_DemoTable(
    Id uniqueidentifier not null
        constraint PK_DemoTable
            primary key clustered,
    Message nvarchar(1000) not null,
    Type uniqueidentifier
        constraint FK_ExampleVersion_T_DemoTable_ExampleVersion_T_DemoTableType
            foreign key (Type) references ExampleVersion.T_DemoTableType (Id),
    Status nvarchar(50) not null
        constraint DF_T_DemoTable_Status
            default 'auto default',
    VersionTimestamp rowversion
)

insert into ExampleVersion.T_DemoTableType (Id, Name) values ('c1060bb2-07b0-4e5d-ad0b-35f3993d823d', 'Eins')
insert into ExampleVersion.T_DemoTableType (Id, Name) values ('d749abff-6a43-4348-839f-61323fdc52d1', 'Zwei')
insert into ExampleVersion.T_DemoTableType (Id, Name) values ('36a072b9-7216-4b99-bf8d-79730a4a1f37', 'Drei')
insert into ExampleVersion.T_DemoTableType (Id, Name) values ('230a5728-acb6-4e91-aea3-05ef34c0755d', 'Vier')
insert into ExampleVersion.T_DemoTable (Id, Message, Type) values (newid(), 'It is working!', '36a072b9-7216-4b99-bf8d-79730a4a1f37')

go

create view ExampleVersion.V_Demo
as select * from ExampleVersion.T_DemoTable