using PTTGC.Prat.Core;

namespace PTTGC.Prat.Backend.Domains;

public static class WorkspaceDomain
{
    public static async Task<Workspace> SubmitWorkspace( Workspace ws )
    {
        // Create workspace in Google Cloud Storage

        // does not perform any analysis

        return ws;
    }

    public static async Task<Workspace> LoadWorkspace( string workspaceId )
    {
        // look for workspace in Google Cloud Storage

        // read and return

        return new Workspace();
    }
}
