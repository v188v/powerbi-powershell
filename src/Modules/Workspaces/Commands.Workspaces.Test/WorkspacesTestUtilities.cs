﻿/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerBI.Commands.Common.Test;
using Microsoft.PowerBI.Common.Abstractions;
using Microsoft.PowerBI.Common.Api.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.PowerBI.Commands.Workspaces.Test
{
    public static class WorkspacesTestUtilities
    {
        public static CmdletInfo GetPowerBIWorkspaceCmdletInfo => new CmdletInfo($"{GetPowerBIWorkspace.CmdletVerb}-{GetPowerBIWorkspace.CmdletName}", typeof(GetPowerBIWorkspace));

        public static void AssertShouldContinueOrganizationTest(Workspace workspace)
        {
            if (workspace == null)
            {
                Assert.Inconclusive("No workspaces returned. Verify you have workspaces in your organization.");
            }
        }

        public static void AssertShouldContinueIndividualTest(Workspace workspace)
        {
            if (workspace == null)
            {
                Assert.Inconclusive("No workspaces returned. Verify you are assigned or own any workspaces.");
            }
        }

        public static Workspace GetWorkspace(System.Management.Automation.PowerShell ps, PowerBIUserScope scope, Guid id)
        {
            var results = InvokeGetPowerBIWorkspace(ps, scope);
            if (results.Any())
            {
                var workspaces = results.Select(x => (Workspace)x.BaseObject);
                return workspaces.First(x => x.Id == id);
            }

            return null;
        }

        public static Workspace GetFirstWorkspace(System.Management.Automation.PowerShell ps, PowerBIUserScope scope)
        {
            var results = InvokeGetPowerBIWorkspace(ps, scope);
            if (results.Any())
            {
                var workspaces = results.Select(x => (Workspace)x.BaseObject);
                return workspaces.First();
            }

            return null;
        }

        // TODO: Until the non-admin endpoint exposes type, this can only call the cmdlet with Organization scope
        public static Workspace GetFirstWorkspaceInOrganization(System.Management.Automation.PowerShell ps)
        {
            var results = InvokeGetPowerBIWorkspace(ps, PowerBIUserScope.Organization);
            if (results.Any())
            {
                var workspaces = results.Select(x => (Workspace)x.BaseObject);
                return workspaces.FirstOrDefault(x => x.Type == WorkspaceType.Workspace);
            }

            return null;
        }

        // TODO: Until the non-admin endpoint supports users, this can only call the cmdlet with Organization scope
        public static Workspace GetFirstWorkspaceWithUsersInOrganization(System.Management.Automation.PowerShell ps)
        {
            var results = InvokeGetPowerBIWorkspace(ps, PowerBIUserScope.Organization);
            if (results.Any())
            {
                var workspaces = results.Select(x => (Workspace)x.BaseObject);
                return workspaces.FirstOrDefault(x => x.Type == WorkspaceType.Workspace && x.Users.Any());
            }

            return null;
        }

        public static Workspace GetFirstDeletedWorkspaceInOrganization(System.Management.Automation.PowerShell ps)
        {
            var results = InvokeGetPowerBIWorkspace(ps, PowerBIUserScope.Organization);
            if (results.Any())
            {
                var workspaces = results.Select(x => (Workspace)x.BaseObject);
                return workspaces.FirstOrDefault(x => x.Type == WorkspaceType.Workspace && x.State == WorkspaceState.Deleted);
            }

            return null;
        }

        private static ICollection<PSObject> InvokeGetPowerBIWorkspace(System.Management.Automation.PowerShell ps, PowerBIUserScope scope)
        {
            var allResults = new List<PSObject>();
            var First = 5000;
            var Skip = 0;
            while (true)
            {
                ps.Commands.Clear();
                var parameters = new Dictionary<string, object>()
                    {
                        { nameof(GetPowerBIWorkspace.Scope), scope.ToString() },
                        { nameof(GetPowerBIWorkspace.First), First },
                        { nameof(GetPowerBIWorkspace.Skip), Skip }
                    };
                ps.AddCommand(GetPowerBIWorkspaceCmdletInfo).AddParameters(parameters);
                var results = ps.Invoke();
                if (results != null)
                {
                    allResults.AddRange(results);
                }
                if (results.Count < First)
                {
                    break;
                }
                Skip += First;
            }
            
            TestUtilities.AssertNoCmdletErrors(ps);
            ps.Commands.Clear();
            return allResults;
        }
    }
}
