import React, { useState, useEffect } from 'react';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';
import { CreditCard, Plus, Percent, ArrowDownLeft, ArrowUpRight, CheckCircle2, ShieldAlert, Building2, Clock, XCircle } from 'lucide-react';

export const AccountsPage = () => {
  const { user } = useAuth();
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // New Account Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [accountType, setAccountType] = useState('Savings');
  const [initialDeposit, setInitialDeposit] = useState(500);

  // Quick Deposit/Withdraw Modal
  const [activeAcc, setActiveAcc] = useState(null);
  const [actionType, setActionType] = useState('');
  const [actionAmount, setActionAmount] = useState(100);
  const [actionDesc, setActionDesc] = useState('');

  // Interest projection
  const [interestData, setInterestData] = useState({});

  const fetchAccounts = async () => {
    setLoading(true);
    try {
      if (user?.customerId) {
        const res = await api.get(`/account/customer/${user.customerId}`);
        setAccounts(res.data);
      } else if (user?.role === 'Admin') {
        const res = await api.get('/account');
        setAccounts(res.data);
      }
    } catch (err) {
      setError('Failed to fetch accounts.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAccounts();
  }, [user]);

  const handleCreateAccount = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      await api.post('/account', {
        customerId: user.customerId || 2,
        accountType: accountType,
        initialDeposit: parseFloat(initialDeposit)
      });
      setSuccess(`Account application submitted! Your ${accountType} account request is now awaiting Admin Approval.`);
      setShowCreateModal(false);
      fetchAccounts();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to apply for account.');
    }
  };

  const handleRequestClosure = async (acc) => {
    if (acc.balance > 0) {
      setError(`Cannot request closure for account ${acc.accountNumber} with an active balance of $${acc.balance.toFixed(2)}. Please withdraw or transfer funds first.`);
      return;
    }
    if (window.confirm(`Submit closure request for account ${acc.accountNumber} to Admin for approval?`)) {
      setError('');
      setSuccess('');
      try {
        await api.post(`/account/${acc.accountNumber}/request-closure`);
        setSuccess(`Account closure request submitted for ${acc.accountNumber}. Awaiting Admin Approval.`);
        fetchAccounts();
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to submit closure request.');
      }
    }
  };

  const handleDepositWithdraw = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    try {
      const endpoint = actionType === 'Deposit' ? '/transaction/deposit' : '/transaction/withdraw';
      await api.post(endpoint, {
        accountNumber: activeAcc.accountNumber,
        amount: parseFloat(actionAmount),
        description: actionDesc || `${actionType} via Accounts Web Portal`
      });
      setSuccess(`Successfully processed ${actionType} of $${actionAmount} for account ${activeAcc.accountNumber}.`);
      setActiveAcc(null);
      fetchAccounts();
    } catch (err) {
      setError(err.response?.data?.message || `Failed to process ${actionType}.`);
    }
  };

  const calculateInterest = async (accNumber) => {
    try {
      const res = await api.get(`/account/${accNumber}/interest`);
      setInterestData(prev => ({ ...prev, [accNumber]: res.data.projectedAnnualInterest }));
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold text-slate-900 flex items-center gap-2">
            <CreditCard className="w-7 h-7 text-blue-700" /> {user?.role === 'Admin' ? 'All Bank Accounts Directory' : 'My Account Portfolios'}
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Managed via SOLID AccountFactory & Dynamic Interest Strategies
          </p>
        </div>

        {user?.role !== 'Admin' && (
          <button
            onClick={() => setShowCreateModal(true)}
            className="px-4 py-2.5 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow transition-all flex items-center gap-2"
          >
            <Plus className="w-4 h-4" /> Open New Account
          </button>
        )}
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 rounded-2xl text-red-700 text-sm font-semibold flex items-center gap-2">
          <ShieldAlert className="w-5 h-5 flex-shrink-0" /> {error}
        </div>
      )}

      {success && (
        <div className="p-4 bg-emerald-50 border border-emerald-200 rounded-2xl text-emerald-800 text-sm font-semibold flex items-center gap-2">
          <CheckCircle2 className="w-5 h-5 flex-shrink-0" /> {success}
        </div>
      )}

      {/* Account Cards Grid */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {accounts.map((acc) => (
          <div
            key={acc.id}
            className="bg-white border border-slate-200 rounded-2xl p-6 shadow-sm flex flex-col justify-between"
          >
            <div>
              <div className="flex items-center justify-between mb-4">
                <span className="px-3 py-1 bg-slate-100 border border-slate-200 text-slate-800 font-mono text-xs font-bold rounded-xl">
                  {acc.accountNumber}
                </span>
                <span className={`px-2.5 py-0.5 text-xs font-bold rounded-full flex items-center gap-1 ${
                  acc.status === 'Active' ? 'bg-emerald-50 border border-emerald-200 text-emerald-800' :
                  acc.status === 'PendingApproval' ? 'bg-amber-50 border border-amber-200 text-amber-800' :
                  acc.status === 'PendingClosure' ? 'bg-red-50 border border-red-200 text-red-800' : 'bg-slate-100 text-slate-600'
                }`}>
                  {acc.status === 'PendingApproval' && <Clock className="w-3 h-3 text-amber-600" />}
                  {acc.status === 'PendingClosure' && <XCircle className="w-3 h-3 text-red-600" />}
                  {acc.status === 'PendingApproval' ? 'Awaiting Approval' : acc.status === 'PendingClosure' ? 'Closure Pending' : acc.status}
                </span>
              </div>

              <div className="text-xs font-bold uppercase tracking-wider text-slate-500">{acc.accountType} Account</div>
              <div className="text-3xl font-extrabold text-slate-900 my-2">
                ${acc.balance.toLocaleString('en-US', { minimumFractionDigits: 2 })}
              </div>

              <div className="space-y-1.5 text-xs text-slate-600 mt-4 pt-4 border-t border-slate-100 font-medium">
                <div className="flex justify-between">
                  <span>Interest Rate:</span>
                  <strong className="text-slate-900">{acc.interestRate}% APR</strong>
                </div>
                <div className="flex justify-between">
                  <span>Overdraft Facility:</span>
                  <strong className="text-amber-700">${acc.overdraftLimit}</strong>
                </div>
                {interestData[acc.accountNumber] !== undefined && (
                  <div className="flex justify-between text-emerald-700 pt-1 font-bold">
                    <span>Projected 1-Yr Interest:</span>
                    <span>+${interestData[acc.accountNumber].toFixed(2)}</span>
                  </div>
                )}
              </div>
            </div>

            <div className="mt-6 pt-4 border-t border-slate-100 space-y-2">
              {acc.status === 'Active' ? (
                <>
                  <div className="grid grid-cols-3 gap-2">
                    <button
                      onClick={() => { setActiveAcc(acc); setActionType('Deposit'); setActionAmount(100); }}
                      className="py-2 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 text-emerald-800 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-1"
                    >
                      <ArrowDownLeft className="w-3.5 h-3.5" /> Deposit
                    </button>
                    <button
                      onClick={() => { setActiveAcc(acc); setActionType('Withdraw'); setActionAmount(100); }}
                      className="py-2 bg-red-50 hover:bg-red-100 border border-red-200 text-red-700 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-1"
                    >
                      <ArrowUpRight className="w-3.5 h-3.5" /> Withdraw
                    </button>
                    <button
                      onClick={() => calculateInterest(acc.accountNumber)}
                      className="py-2 bg-blue-50 hover:bg-blue-100 border border-blue-200 text-blue-800 rounded-xl text-xs font-bold transition-all flex items-center justify-center gap-1"
                    >
                      <Percent className="w-3.5 h-3.5" /> Interest
                    </button>
                  </div>

                  {user?.role !== 'Admin' && (
                    <button
                      onClick={() => handleRequestClosure(acc)}
                      className="w-full py-1.5 bg-slate-50 hover:bg-red-50 border border-slate-200 hover:border-red-200 text-slate-600 hover:text-red-700 rounded-xl text-[11px] font-bold transition-colors text-center"
                    >
                      Request Account Closure
                    </button>
                  )}
                </>
              ) : (
                <div className="p-3 bg-amber-50 border border-amber-200 rounded-xl text-amber-800 text-xs font-medium text-center">
                  ⏳ {acc.status === 'PendingApproval' ? 'Awaiting Admin Approval' : 'Closure Request Under Admin Review'}
                </div>
              )}
            </div>
          </div>
        ))}
      </div>

      {/* Create Account Modal */}
      {showCreateModal && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              <Building2 className="w-5 h-5 text-blue-700" /> Apply to Open Bank Account
            </h3>
            <p className="text-xs text-slate-500 font-medium">
              Your application will be submitted to Admin for approval.
            </p>

            <form onSubmit={handleCreateAccount} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Account Type</label>
                <select
                  value={accountType}
                  onChange={(e) => setAccountType(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                >
                  <option value="Savings">Savings Account (4.5% APR, Min $100)</option>
                  <option value="Checking">Checking Account (0.5% APR, $500 Overdraft)</option>
                  <option value="FixedDeposit">Fixed Deposit (7.0% APR, Min $1,000)</option>
                </select>
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Initial Deposit Amount ($)</label>
                <input
                  type="number"
                  required
                  min="1"
                  value={initialDeposit}
                  onChange={(e) => setInitialDeposit(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setShowCreateModal(false)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="flex-1 py-2 bg-blue-700 hover:bg-blue-800 text-white rounded-xl text-sm font-bold shadow-md"
                >
                  Submit Application
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Deposit / Withdraw Modal */}
      {activeAcc && (
        <div className="fixed inset-0 bg-slate-900/60 backdrop-blur-xs z-50 flex items-center justify-center p-4">
          <div className="bg-white border border-slate-200 rounded-2xl max-w-md w-full p-6 shadow-2xl space-y-4">
            <h3 className="text-xl font-bold text-slate-900 flex items-center gap-2">
              {actionType === 'Deposit' ? <ArrowDownLeft className="w-5 h-5 text-emerald-600" /> : <ArrowUpRight className="w-5 h-5 text-red-600" />}
              {actionType} Funds — {activeAcc.accountNumber}
            </h3>

            <form onSubmit={handleDepositWithdraw} className="space-y-4">
              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Amount ($)</label>
                <input
                  type="number"
                  required
                  min="0.01"
                  step="0.01"
                  value={actionAmount}
                  onChange={(e) => setActionAmount(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-slate-700 mb-1">Description / Memo</label>
                <input
                  type="text"
                  value={actionDesc}
                  onChange={(e) => setActionDesc(e.target.value)}
                  className="w-full px-3 py-2 bg-white border border-slate-300 rounded-xl text-sm text-slate-900 focus:outline-none focus:border-blue-700"
                  placeholder="e.g. Salary Credit / ATM Cash"
                />
              </div>

              <div className="flex gap-3 pt-2">
                <button
                  type="button"
                  onClick={() => setActiveAcc(null)}
                  className="flex-1 py-2 bg-slate-100 hover:bg-slate-200 text-slate-700 rounded-xl text-sm font-bold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className={`flex-1 py-2 text-white rounded-xl text-sm font-bold shadow-md ${
                    actionType === 'Deposit' ? 'bg-emerald-700 hover:bg-emerald-800' : 'bg-red-700 hover:bg-red-800'
                  }`}
                >
                  Execute {actionType}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
