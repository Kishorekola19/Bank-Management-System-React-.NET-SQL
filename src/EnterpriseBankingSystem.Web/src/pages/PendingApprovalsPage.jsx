import React, { useState, useEffect } from 'react';
import api from '../services/api';
import { ShieldCheck, CheckCircle2, XCircle, Clock, Building2, CreditCard, Landmark, RefreshCw, AlertCircle } from 'lucide-react';

export const PendingApprovalsPage = () => {
  const [data, setData] = useState({
    pendingAccountOpenings: [],
    pendingAccountClosures: [],
    pendingCards: [],
    pendingLoans: []
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const fetchPendingRequests = async () => {
    setLoading(true);
    setError('');
    try {
      const res = await api.get('/adminapprovals/pending-requests');
      setData(res.data);
    } catch (err) {
      console.error(err);
      setError('Failed to fetch pending customer requests.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchPendingRequests();
  }, []);

  const handleApproveAccount = async (accountId, approve) => {
    setError('');
    setSuccess('');
    try {
      await api.post(`/adminapprovals/approve-account/${accountId}?approve=${approve}`);
      setSuccess(`Account #${accountId} has been ${approve ? 'APPROVED and activated' : 'REJECTED'}.`);
      fetchPendingRequests();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to process account request.');
    }
  };

  const handleApproveClosure = async (accountId, approve) => {
    setError('');
    setSuccess('');
    try {
      await api.post(`/adminapprovals/approve-account-closure/${accountId}?approve=${approve}`);
      setSuccess(`Account closure request for #${accountId} has been ${approve ? 'APPROVED and closed' : 'REJECTED (Account reactivated)'}.`);
      fetchPendingRequests();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to process account closure request.');
    }
  };

  const handleApproveCard = async (cardId, approve) => {
    setError('');
    setSuccess('');
    try {
      await api.post(`/adminapprovals/approve-card/${cardId}?approve=${approve}`);
      setSuccess(`ATM Card #${cardId} issuance request has been ${approve ? 'APPROVED and issued' : 'REJECTED'}.`);
      fetchPendingRequests();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to process ATM card request.');
    }
  };

  const handleApproveLoan = async (loanId, approve) => {
    setError('');
    setSuccess('');
    try {
      await api.post(`/adminapprovals/approve-loan/${loanId}?approve=${approve}`);
      setSuccess(`Loan Application #${loanId} has been ${approve ? 'APPROVED and credited' : 'REJECTED'}.`);
      fetchPendingRequests();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to process loan request.');
    }
  };

  const totalPending = (data.pendingAccountOpenings?.length || 0) +
                       (data.pendingAccountClosures?.length || 0) +
                       (data.pendingCards?.length || 0) +
                       (data.pendingLoans?.length || 0);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
            <ShieldCheck className="w-7 h-7 text-blue-700" /> Admin Pending Requests & Approvals
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Managerial oversight log for client account openings, closures, debit cards, and loan applications
          </p>
        </div>

        <button
          onClick={fetchPendingRequests}
          className="px-4 py-2.5 bg-white hover:bg-slate-50 text-slate-700 rounded-xl text-sm font-bold border border-slate-200 shadow-xs transition-all flex items-center gap-2"
        >
          <RefreshCw className="w-4 h-4" /> Refresh Requests Log
        </button>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-2xl text-red-700 text-sm font-semibold flex items-center gap-2">
          <AlertCircle className="w-5 h-5 flex-shrink-0" /> {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl text-emerald-800 text-sm font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5 flex-shrink-0" /> {success}
        </div>
      )}

      {totalPending === 0 && !loading && (
        <div className="bg-white border border-slate-200 rounded-2xl p-12 text-center shadow-sm">
          <CheckCircle2 className="w-12 h-12 text-emerald-600 mx-auto mb-3" />
          <h3 className="text-lg font-bold text-slate-900">All Customer Requests Processed</h3>
          <p className="text-sm text-slate-500 mt-1">There are currently no pending customer approvals in queue.</p>
        </div>
      )}

      {/* 1. Account Opening Approvals */}
      {data.pendingAccountOpenings?.length > 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <Building2 className="w-5 h-5 text-blue-700" /> Account Opening Requests ({data.pendingAccountOpenings.length})
            </h2>
            <span className="px-2.5 py-0.5 bg-amber-50 text-amber-800 border border-amber-200 text-xs font-bold rounded-full">
              Pending Admin Action
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Account #</th>
                  <th className="py-3 px-4">Customer Name</th>
                  <th className="py-3 px-4">Account Type</th>
                  <th className="py-3 px-4">Initial Deposit</th>
                  <th className="py-3 px-4">Requested On</th>
                  <th className="py-3 px-4 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.pendingAccountOpenings.map(acc => (
                  <tr key={acc.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-blue-700">{acc.accountNumber}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">{acc.customerName}</td>
                    <td className="py-3 px-4 text-xs font-semibold text-slate-700">{acc.accountType}</td>
                    <td className="py-3 px-4 font-bold text-emerald-700">${acc.balance.toLocaleString()}</td>
                    <td className="py-3 px-4 text-xs font-mono text-slate-500">{new Date(acc.createdAt).toLocaleDateString()}</td>
                    <td className="py-3 px-4 text-center">
                      <div className="flex gap-2 justify-center">
                        <button
                          onClick={() => handleApproveAccount(acc.id, true)}
                          className="px-3 py-1 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5" /> Approve
                        </button>
                        <button
                          onClick={() => handleApproveAccount(acc.id, false)}
                          className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <XCircle className="w-3.5 h-3.5" /> Reject
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* 2. Account Closure Approvals */}
      {data.pendingAccountClosures?.length > 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <XCircle className="w-5 h-5 text-red-600" /> Account Closure Requests ({data.pendingAccountClosures.length})
            </h2>
            <span className="px-2.5 py-0.5 bg-red-50 text-red-800 border border-red-200 text-xs font-bold rounded-full">
              Pending Closure Approval
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Account #</th>
                  <th className="py-3 px-4">Customer Name</th>
                  <th className="py-3 px-4">Account Type</th>
                  <th className="py-3 px-4">Balance</th>
                  <th className="py-3 px-4 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.pendingAccountClosures.map(acc => (
                  <tr key={acc.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-red-700">{acc.accountNumber}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">{acc.customerName}</td>
                    <td className="py-3 px-4 text-xs font-semibold text-slate-700">{acc.accountType}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">${acc.balance.toLocaleString()}</td>
                    <td className="py-3 px-4 text-center">
                      <div className="flex gap-2 justify-center">
                        <button
                          onClick={() => handleApproveClosure(acc.id, true)}
                          className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5" /> Approve Closure
                        </button>
                        <button
                          onClick={() => handleApproveClosure(acc.id, false)}
                          className="px-3 py-1 bg-slate-600 hover:bg-slate-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <XCircle className="w-3.5 h-3.5" /> Reject Request
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* 3. ATM Card Issuance Approvals */}
      {data.pendingCards?.length > 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <CreditCard className="w-5 h-5 text-blue-700" /> ATM Card Issuance Requests ({data.pendingCards.length})
            </h2>
            <span className="px-2.5 py-0.5 bg-blue-50 text-blue-800 border border-blue-200 text-xs font-bold rounded-full">
              Awaiting Manager Sign-off
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Card #</th>
                  <th className="py-3 px-4">Cardholder</th>
                  <th className="py-3 px-4">Card Type</th>
                  <th className="py-3 px-4">Linked Account</th>
                  <th className="py-3 px-4">Daily Limit</th>
                  <th className="py-3 px-4 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.pendingCards.map(card => (
                  <tr key={card.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-slate-900">{card.cardNumber}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">{card.cardHolderName}</td>
                    <td className="py-3 px-4 text-xs font-semibold text-blue-800">{card.cardType}</td>
                    <td className="py-3 px-4 font-mono text-xs text-slate-600">{card.accountNumber}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">${card.dailyWithdrawalLimit}</td>
                    <td className="py-3 px-4 text-center">
                      <div className="flex gap-2 justify-center">
                        <button
                          onClick={() => handleApproveCard(card.id, true)}
                          className="px-3 py-1 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5" /> Issue Card
                        </button>
                        <button
                          onClick={() => handleApproveCard(card.id, false)}
                          className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <XCircle className="w-3.5 h-3.5" /> Reject
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* 4. Loan Application Approvals */}
      {data.pendingLoans?.length > 0 && (
        <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-4">
          <div className="flex items-center justify-between pb-3 border-b border-slate-100">
            <h2 className="text-base font-bold text-slate-900 flex items-center gap-2">
              <Landmark className="w-5 h-5 text-blue-700" /> Loan Applications ({data.pendingLoans.length})
            </h2>
            <span className="px-2.5 py-0.5 bg-amber-50 text-amber-800 border border-amber-200 text-xs font-bold rounded-full">
              Credit Risk Assessment Pending
            </span>
          </div>

          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm text-slate-700">
              <thead className="bg-slate-50 text-xs font-bold uppercase text-slate-500 border-b border-slate-200">
                <tr>
                  <th className="py-3 px-4">Loan #</th>
                  <th className="py-3 px-4">Customer</th>
                  <th className="py-3 px-4">Loan Type</th>
                  <th className="py-3 px-4">Principal</th>
                  <th className="py-3 px-4">Tenure</th>
                  <th className="py-3 px-4">Monthly EMI</th>
                  <th className="py-3 px-4 text-center">Action</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {data.pendingLoans.map(loan => (
                  <tr key={loan.id} className="hover:bg-slate-50">
                    <td className="py-3 px-4 font-mono text-xs font-bold text-blue-700">{loan.loanNumber}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">{loan.customerName}</td>
                    <td className="py-3 px-4 text-xs font-semibold text-slate-700">{loan.loanType}</td>
                    <td className="py-3 px-4 font-bold text-slate-900">${loan.principalAmount.toLocaleString()}</td>
                    <td className="py-3 px-4 text-xs font-medium">{loan.tenureMonths} Months</td>
                    <td className="py-3 px-4 font-bold text-emerald-700">${loan.monthlyEMI}</td>
                    <td className="py-3 px-4 text-center">
                      <div className="flex gap-2 justify-center">
                        <button
                          onClick={() => handleApproveLoan(loan.id, true)}
                          className="px-3 py-1 bg-emerald-600 hover:bg-emerald-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <CheckCircle2 className="w-3.5 h-3.5" /> Approve & Credit
                        </button>
                        <button
                          onClick={() => handleApproveLoan(loan.id, false)}
                          className="px-3 py-1 bg-red-600 hover:bg-red-700 text-white rounded-lg text-xs font-bold shadow-xs flex items-center gap-1"
                        >
                          <XCircle className="w-3.5 h-3.5" /> Reject Loan
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};
