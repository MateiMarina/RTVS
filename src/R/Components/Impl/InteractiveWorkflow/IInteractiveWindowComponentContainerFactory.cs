﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IInteractiveWindowComponentContainerFactory {
        IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator, IRSessionProvider sessionProvider);
    }
}