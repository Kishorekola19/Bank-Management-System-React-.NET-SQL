import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { Wallet, CreditCard, Landmark, ArrowUpRight, ArrowDownLeft, ArrowRightLeft, Plus, RefreshCw, ShieldCheck, Building2, FileCheck, Clock } from 'lucide-react';

export const DashboardPage = ({ setActiveTab }) => {
  const { user } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [transactions, setTransactions] = useState([]);
  const [loans, setLoans] = useState([]);
  const [pendingCount, setPendingCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchData = async () => {
    setLoading(true);
    setError('');
    try {
      if (user?.customerId) {
        const accRes = await api.get(`/account/customer/${user.customerId}`);
        setAccounts(accRes.data);

        const loanRes = await api.get(`/loan/customer/${user.customerId}`);
        setLoans(loanRes.data);

        if (accRes.data.length > 0) {
          const txRes = await api.get(`/transaction/account/${accRes.data[0].accountNumber}`);
          setTransactions(txRes.data);
        }
      } else if (user?.role === 'Admin') {
        const accRes = await api.get('/account');
        setAccounts(accRes.data);

        const txRes = await api.get('/transaction/all');
        setTransactions(txRes.data);

        const loanRes = await api.get('/loan/all');
        setLoans(loanRes.data);

        const pendingRes = await api.get('/adminapprovals/pending-requests');
        const pCount = (pendingRes.data.pendingAccountOpenings?.length || 0) +
                       (pendingRes.data.pendingAccountClosures?.length || 0) +
                       (pendingRes.data.pendingCards?.length || 0) +
                       (pendingRes.data.pendingLoans?.length || 0);
        setPendingCount(pCount);
      }
    } catch (err) {
      console.error(err);
      setError('Failed to load dashboard metrics.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [user]);

  const totalBalance = accounts.reduce((acc, curr) => acc + (curr.balance || 0), 0);
  const totalLoanBalance = loans.reduce((acc, curr) => acc + (curr.remainingAmount || 0), 0);

  return (
    <div className="space-y-6">
      {/* Top Banner */}
      <div className="bg-slate-900 text-white p-6 rounded-2xl border border-slate-800 shadow-md flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <div className="inline-flex items-center gap-1.5 px-3 py-1 bg-blue-900/60 border border-blue-700/50 rounded-md text-xs font-bold text-blue-300 mb-2">
            <Building2 className="w-3.5 h-3.5" /> Apex Enterprise Bank Portal
          </div>
          <h1 className="text-2xl font-extrabold tracking-tight">Welcome back, {user?.fullName}!</h1>
          <p className="text-sm text-slate-300 mt-1">
            {user?.role === 'Admin' ? 'System Administrator Portal — Full Executive & Approval Oversight' : `Customer Account #${user?.customerId || 'N/A'}`}
          </p>
        </div>

        <div className="flex items-center gap-3">
          {user?.role === 'Admin' ? (
            <button
              onClick={() => setActiveTab('approvals')}
              className="px-4 py-2.5 bg-amber-600 hover:bg-amber-700 text-white rounded-xl text-sm font-bold shadow transition-all flex items-center gap-2"
            >
              <FileCheck className="w-4 h-4" /> Review Pending Requests ({pendingCount})
            </button>
          ) : (
            <>
              <button
                onClick={() => setActiveTab('transfer')}
                className="px-4 py-2.5 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow transition-all flex items-center gap-2"
              >
                <ArrowRightLeft className="w-4 h-4" /> Fund Transfer
              </button>
              <button
                onClick={() => setActiveTab('accounts')}
                className="px-4 py-2.5 bg-slate-800 hover:bg-slate-700 text-slate-200 rounded-xl text-sm font-bold border border-slate-700 transition-all flex items-center gap-2"
              >
                <Plus className="w-4 h-4" /> Open Account
              </button>
            </>
          )}
          <button
            onClick={fetchData}
            className="p-2.5 bg-slate-800 hover:bg-slate-700 text-slate-300 rounded-xl border border-slate-700 transition-colors"
            title="Refresh Data"
          >
            <RefreshCw className="w-4 h-4" />
          </button>
        </div>
      </div>

      {/* Metrics Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
          <div className="flex items-center justify-between text-slate-500 mb-3">
            <span className="text-xs font-bold uppercase tracking-wider">{user?.role === 'Admin' ? 'System Net Deposits' : 'Total Net Balance'}</span>
            <div className="p-2 bg-emerald-50 text-emerald-700 rounded-xl border border-emerald-100">
              <Wallet className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900">${totalBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}</div>
          <div className="text-xs text-slate-500 mt-1 font-medium">Across {accounts.length} bank account(s)</div>
        </div>

        {user?.role === 'Admin' ? (
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm cursor-pointer hover:border-amber-400 transition-all" onClick={() => setActiveTab('approvals')}>
            <div className="flex items-center justify-between text-slate-500 mb-3">
              <span className="text-xs font-bold uppercase tracking-wider">Pending Approvals</span>
              <div className="p-2 bg-amber-50 text-amber-700 rounded-xl border border-amber-100">
                <Clock className="w-5 h-5" />
              </div>
            </div>
            <div className="text-2xl font-extrabold text-amber-700">{pendingCount} Requests</div>
            <div className="text-xs text-slate-500 mt-1 font-medium">Accounts, Cards & Loans pending sign-off</div>
          </div>
        ) : (
          <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
            <div className="flex items-center justify-between text-slate-500 mb-3">
              <span className="text-xs font-bold uppercase tracking-wider">Active Accounts</span>
              <div className="p-2 bg-blue-50 text-blue-700 rounded-xl border border-blue-100">
                <CreditCard className="w-5 h-5" />
              </div>
            </div>
            <div className="text-2xl font-extrabold text-slate-900">{accounts.length}</div>
            <div className="text-xs text-slate-500 mt-1 font-medium">Savings, Checking & FD</div>
          </div>
        )}

        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
          <div className="flex items-center justify-between text-slate-500 mb-3">
            <span className="text-xs font-bold uppercase tracking-wider">Outstanding Loans</span>
            <div className="p-2 bg-amber-50 text-amber-700 rounded-xl border border-amber-100">
              <Landmark className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900">${totalLoanBalance.toLocaleString('en-US', { minimumFractionDigits: 2 })}</div>
          <div className="text-xs text-slate-500 mt-1 font-medium">{loans.length} loan portfolio(s)</div>
        </div>

        <div className="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm">
          <div className="flex items-center justify-between text-slate-500 mb-3">
            <span className="text-xs font-bold uppercase tracking-wider">Total Transactions</span>
            <div className="p-2 bg-slate-100 text-slate-700 rounded-xl border border-slate-200">
              <ArrowRightLeft className="w-5 h-5" />
            </div>
          </div>
          <div className="text-2xl font-extrabold text-slate-900">{transactions.length}</div>
          <div className="text-xs text-slate-500 mt-1 font-medium">Recorded transaction history</div>
        </div>
      </div>

      {/* Account Cards Section */}
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <CreditCard className="w-5 h-5 text-blue-700" /> Account Portfolios
          </h2>
          <button
            onClick={() => setActiveTab('accounts')}
            className="text-xs font-bold text-blue-700 hover:text-blue-800"
          >
            Manage Accounts →
          </button>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {accounts.slice(0, 6).map((acc) => (
            <div
              key={acc.id}
              className="bg-white border border-slate-200 hover:border-slate-300 p-5 rounded-2xl shadow-sm transition-all"
            >
              <div className="flex items-center justify-between mb-3">
                <span className="px-2.5 py-1 bg-slate-100 border border-slate-200 rounded-lg text-xs font-mono font-bold text-slate-800">
                  {acc.accountNumber}
                </span>
                <span className={`px-2.5 py-0.5 rounded-full text-[11px] font-bold ${
                  acc.status === 'Active' ? 'bg-emerald-50 text-emerald-800 border border-emerald-200' :
                  acc.status === 'PendingApproval' ? 'bg-amber-50 text-amber-800 border border-amber-200' : 'bg-red-50 text-red-800 border border-red-200'
                }`}>
                  {acc.status === 'PendingApproval' ? 'Awaiting Approval' : acc.status}
                </span>
              </div>

              <div className="text-xs text-slate-500 font-bold uppercase tracking-wider">{acc.accountType} Account</div>
              <div className="text-2xl font-extrabold text-slate-900 my-1">
                ${acc.balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </div>

              <div className="mt-4 pt-3 border-t border-slate-100 flex items-center justify-between text-xs text-slate-600 font-medium">
                <span>Interest Rate: <strong className="text-slate-900">{acc.interestRate}% APR</strong></span>
                {acc.overdraftLimit > 0 && (
                  <span>Overdraft: <strong className="text-amber-700">${acc.overdraftLimit}</strong></span>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Recent Activity Table */}
      <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-slate-900 flex items-center gap-2">
            <ArrowRightLeft className="w-5 h-5 text-blue-700" /> Recent System Transactions
          </h2>
        </div>

        {transactions.length === 0 ? (
          <div className="text-center py-8 text-slate-500 text-sm">No transaction records found.</div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Ref #</th>
                  <th className="py-3 px-4">Type</th>
                  <th className="py-3 px-4">Source / Target</th>
                  <th className="py-3 px-4">Description</th>
                  <th className="py-3 px-4 text-right">Amount</th>
                  <th className="py-3 px-4">Timestamp</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {transactions.slice(0, 5).map((tx) => (
                  <tr key={tx.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-blue-700">{tx.transactionReference}</td>
                    <td className="py-3 px-4">
                      <span className={`inline-flex items-center gap-1 px-2 py-0.5 rounded text-xs font-bold ${
                        tx.transactionType === 'Deposit' ? 'bg-emerald-50 text-emerald-700 border border-emerald-200' :
                        tx.transactionType === 'Withdrawal' ? 'bg-red-50 text-red-700 border border-red-200' : 'bg-blue-50 text-blue-700 border border-blue-200'
                      }`}>
                        {tx.transactionType === 'Deposit' && <ArrowDownLeft className="w-3 h-3" />}
                        {tx.transactionType === 'Withdrawal' && <ArrowUpRight className="w-3 h-3" />}
                        {tx.transactionType === 'Transfer' && <ArrowRightLeft className="w-3 h-3" />}
                        {tx.transactionType}
                      </span>
                    </td>
                    <td className="py-3 px-4 text-xs font-mono text-slate-600">
                      {tx.sourceAccountNumber} {tx.targetAccountNumber ? `→ ${tx.targetAccountNumber}` : ''}
                    </td>
                    <td className="py-3 px-4 text-xs text-slate-700 font-medium">{tx.description}</td>
                    <td className="py-3 px-4 text-right font-bold font-mono text-slate-900">
                      ${tx.amount.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                    </td>
                    <td className="py-3 px-4 text-xs text-slate-500 font-medium">
                      {new Date(tx.timestamp).toLocaleString()}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
};
