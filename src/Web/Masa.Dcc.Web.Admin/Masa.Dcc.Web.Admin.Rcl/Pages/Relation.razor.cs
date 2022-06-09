// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Dcc.Web.Admin.Rcl.Pages
{
    public partial class Relation
    {
        [Parameter]
        public AppDetailModel AppDetail { get; set; } = null!;

        private bool _showRelationModal;
        private List<StringNumber> _selectEnvClusterIds;
        private ConfigObjectModel _configObject = new();
    }
}
