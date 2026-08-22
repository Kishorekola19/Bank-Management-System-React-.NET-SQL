import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { ArrowRightLeft, ShieldCheck, CheckCircle2, ShieldAlert, Zap } from 'lucide-react';

export const TransferPage = () => {
  const { user } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [sourceAccount, setSourceAccount] = useState('');
  const [targetAccount, setTargetAccount] = useState('');
  const [amount, setAmount] = useState(250);
  const [description, setDescription] = useState('Fund transfer');
  
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [successTx, setSuccessTx] = useState(null);

  useEffect(() => {
    const loadAccounts = async () => {
      try {
        if (user?.customerId) {
          const res = await api.get(`/account/customer/${user.customerId}`);
          setAccounts(res.data);
          if (res.data.length > 0) {
            setSourceAccount(res.data[0].accountNumber);
          }
        }
      } catch (err) {
        console.error(err);
      }
    };
    loadAccounts();
  }, [user]);

  const handleTransfer = async (e) => {
    e.preventDefault();
    setError('');
    setSuccessTx(null);
    setLoading(true);

    try {
      const res = await api.post('/transaction/transfer', {
        sourceAccountNumber: sourceAccount,
        targetAccountNumber: targetAccount,
        amount: parseFloat(amount),
        description: description || 'Instant Fund Transfer'
      });
      setSuccessTx(res.data);

      const refreshed = await api.get(`/account/customer/${user.customerId}`);
      setAccounts(refreshed.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Transfer failed. Please check account details.');
    } finally {
      setLoading(false);
    }
  };

  const selectedAccObj = accounts.find(a => a.accountNumber === sourceAccount);

  return (
    <div className="max-w-2xl mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
          <ArrowRightLeft className="w-7 h-7 text-blue-700" /> ACID-Compliant Fund Transfer
        </h1>
        <p className="text-sm text-slate-600 mt-1">
          Atomic database transactions using TransferTransactionStrategy with optimistic concurrency control.
        </p>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-2xl text-red-700 text-sm font-semibold flex items-center gap-2">
          <ShieldAlert className="w-5 h-5 flex-shrink-0" /> {error}
        </div>
      )}

      {successTx && (
        <div className="p-6 bg-emerald-50 border border-emerald-200 rounded-2xl text-emerald-900 space-y-2 shadow-sm">
          <div className="flex items-center gap-2 text-emerald-800 font-bold text-lg">
            <CheckCircle2 className="w-6 h-6 text-emerald-600" /> Transaction Completed Successfully!
          </div>
          <div className="text-xs space-y-1 pt-2 border-t border-emerald-200 font-mono text-slate-700">
            <div>Reference #: <strong className="text-slate-900">{successTx.transactionReference}</strong></div>
            <div>Amount Transferred: <strong className="text-slate-900">${successTx.amount?.toFixed(2)}</strong></div>
            <div>Source Account: <strong className="text-slate-900">{successTx.sourceAccountNumber}</strong></div>
            <div>Target Account: <strong className="text-slate-900">{successTx.targetAccountNumber}</strong></div>
            <div>Timestamp: <strong className="text-slate-900">{new Date(successTx.timestamp).toLocaleString()}</strong></div>
          </div>
        </div>
      )}

      <div className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm space-y-6">
        <form onSubmit={handleTransfer} className="space-y-4">
          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Source Account</label>
            <select
              value={sourceAccount}
              onChange={(e) => setSourceAccount(e.target.value)}
              className="w-full px-3 py-2.5 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
            >
              {accounts.map(acc => (
                <option key={acc.id} value={acc.accountNumber}>
                  {acc.accountNumber} ({acc.accountType} — Balance: ${acc.balance.toFixed(2)})
                </option>
              ))}
            </select>
            {selectedAccObj && (
              <div className="text-[11px] text-slate-500 mt-1 font-medium">
                Available Transfer Limit: ${(selectedAccObj.balance + selectedAccObj.overdraftLimit).toFixed(2)}
              </div>
            )}
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Target Account Number</label>
            <input
              type="text"
              required
              value={targetAccount}
              onChange={(e) => setTargetAccount(e.target.value)}
              className="w-full px-3 py-2.5 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
              placeholder="e.g. CHK-9182746352 or SAV-..."
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Transfer Amount ($)</label>
            <input
              type="number"
              required
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(e) => setAmount(e.target.value)}
              className="w-full px-3 py-2.5 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 font-mono focus:outline-none focus:border-blue-700"
            />
          </div>

          <div>
            <label className="block text-xs font-bold text-slate-700 mb-1">Transfer Purpose / Description</label>
            <input
              type="text"
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full px-3 py-2.5 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
              placeholder="e.g. Rent payment / Peer transfer"
            />
          </div>

          <div className="pt-2">
            <button
              type="submit"
              disabled={loading}
              className="w-full py-3 px-4 bg-blue-700 hover:bg-blue-800 text-white font-bold rounded-xl text-sm shadow-md transition-all flex items-center justify-center gap-2"
            >
              {loading ? (
                <div className="w-5 h-5 border-2 border-white border-t-transparent rounded-full animate-spin"></div>
              ) : (
                <>
                  <Zap className="w-4 h-4" /> Confirm Atomic Money Transfer
                </>
              )}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};
