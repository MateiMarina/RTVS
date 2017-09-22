﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LanguageServer.VsCode.Contracts;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Completions;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.Completions;
using Microsoft.R.Editor.Document;
using Microsoft.R.LanguageServer.Completions;
using Microsoft.R.LanguageServer.Extensions;
using Microsoft.R.LanguageServer.Text;

namespace Microsoft.R.LanguageServer.Documents {
    internal sealed class DocumentEntry : IDisposable {
        private readonly IServiceContainer _services;
        private readonly CompletionManager _completionManager;
        private readonly SignatureManager _signatureManager;

        public IEditorView View { get; }
        public IEditorBuffer EditorBuffer { get; }
        public IREditorDocument Document { get; }

        public DocumentEntry(string content, IServiceContainer services) {
            _services = services;

            EditorBuffer = new EditorBuffer(content, "R");
            View = new EditorView(EditorBuffer);
            Document = new REditorDocument(EditorBuffer, services, false);
            _completionManager = new CompletionManager(services);
            _signatureManager = new SignatureManager(services);
        }

        public void ProcessChanges(ICollection<TextDocumentContentChangeEvent> contentChanges) {
            foreach (var change in contentChanges) {
                if (!change.HasRange) {
                    continue;
                }
                var position = EditorBuffer.ToStreamPosition(change.Range.Start);
                var range = new TextRange(position, change.RangeLength);
                if (!string.IsNullOrEmpty(change.Text)) {
                    // Insert or replace
                    if (change.RangeLength == 0) {
                        EditorBuffer.Insert(position, change.Text);
                    } else {
                        EditorBuffer.Replace(range, change.Text);
                    }
                } else {
                    EditorBuffer.Delete(range);
                }
            }
        }

        public void Dispose() => Document?.Close();

        public CompletionList GetCompletions(Position position)
            => _completionManager.GetCompletions(CreateContext(position));

        public async Task<SignatureHelp> GetSignatureHelpAsync(Position position) {
            var signatures = await _signatureManager.GetSignaturesAsync(CreateContext(position));
            return signatures != null ? new SignatureHelp(signatures) : null;
        }

        public Task<Hover> GetHoverAsync(Position position, CancellationToken ct)
            => _signatureManager.GetHoverAsync(CreateContext(position), ct);

        private IRIntellisenseContext CreateContext(Position position) {
            var bufferPosition = EditorBuffer.ToStreamPosition(position);
            var session = new EditorIntellisenseSession(View);
            return new RIntellisenseContext(session, EditorBuffer, Document.EditorTree, bufferPosition);
        }
    }
}
