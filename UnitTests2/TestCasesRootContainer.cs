﻿/*
 * [The "BSD license"]
 * Copyright (c) 2016 Peter Hoch
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using DecisionTableCreator.TestCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests2
{
    public class TestCasesRootContainer : IDisposable
    {
        public TestCasesRoot TestCasesRoot { get; set; }

        public int ConditionChangeCount { get; set; }

        public int ActionChangeCount { get; set; }
    

        public TestCasesRootContainer()
        {
            TestCasesRoot = TestCasesRoot.CreateSimpleTable();
            TestCasesRoot.ActionsBeginChange += TestCasesRootOnActionsChanged;
            TestCasesRoot.ActionsEndChange += TestCasesRootOnActionsChanged;
            TestCasesRoot.ConditionsBeginChange += TestCasesRootOnConditionsChanged;
            TestCasesRoot.ConditionsEndChange += TestCasesRootOnConditionsChanged;
        }

        private void TestCasesRootOnConditionsChanged()
        {
            ConditionChangeCount++;
        }

        private void TestCasesRootOnActionsChanged()
        {
            ActionChangeCount++;
        }

        public void Dispose()
        {
            TestCasesRoot.ActionsBeginChange -= TestCasesRootOnActionsChanged;
            TestCasesRoot.ActionsEndChange -= TestCasesRootOnActionsChanged;
            TestCasesRoot.ConditionsBeginChange -= TestCasesRootOnConditionsChanged;
            TestCasesRoot.ConditionsEndChange -= TestCasesRootOnConditionsChanged;
        }
    }
}
