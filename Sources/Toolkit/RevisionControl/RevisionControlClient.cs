using System;
using System.Collections.Generic;

namespace BenjaminHamon.DevelopmentToolkit.Toolkit.RevisionControl
{
    /// <summary>Client to interface with a revision control service.</summary>
    public interface RevisionControlClient
    {
        /// <summary>Get the active revision in the repository</summary>
        string GetCurrentRevision();

        /// <summary>Generate a revision identifier for the current state, including local changes.</summary>
        string GenerateRevisionWithLocalChanges();

        /// <summary>Get the active branch in the repository, if there is one</summary>
        string GetCurrentBranch();

        /// <summary>Resolve a reference into an exact revision identifier, or return null if it cannot</summary>
        string TryResolveRevision(string reference);

        /// <summary>Resolve a reference into an exact revision identifier, or throw an exception if it cannot</summary>
        string ResolveRevision(string reference);

        /// <summary>Resolve a reference into its exact number value, or to some numeric representation if possible</summary>
        int ResolveRevisionAsNumeric(string reference);

        /// <summary>Convert a revision to its shorter form if possible.</summary>
		string ConvertRevisionToRevisionShort(string revision);

        /// <summary>Get the date associated with a revision</summary>
        DateTime GetRevisionDate(string revision);

        /// <summary>Return a list of revisions, with a starting point in the history and a limit of ancestors</summary>
        List<string> ListRevisions(string start = null, int limit = 100);

        /// <summary>Compare two revision identifiers</summary>
        bool AreRevisionsEqual(string first, string second);
    }
}
