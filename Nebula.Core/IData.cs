using MongoDB.Bson;

namespace Nebula.Core;

public interface IData
{
    BsonDocument Document { get; set; }

    /// <summary>
    /// Update this document with the data from the database.
    /// This document will be overwritten and all modifications will be lost.
    /// </summary>
    void Update();

    /// <summary>
    /// Submit this data document to the database.
    /// </summary>
    void Submit();
}